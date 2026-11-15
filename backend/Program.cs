using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;
using RagLab.DependencyInjection;
using RagLab.Documents;
using RagLab.Llm;
using RagLab.Rag;
using RagLab.Search;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRagLab(builder.Configuration);
builder.Services.AddHealthChecks();
builder.Services.AddCors(o => o.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));
var app = builder.Build();

app.Use(async (context, next) =>
{
    var stopwatch = Stopwatch.StartNew();
    try
    {
        await next();
    }
    finally
    {
        app.Logger.LogInformation(
            "{Method} {Path} -> {StatusCode} in {ElapsedMs} ms",
            context.Request.Method, context.Request.Path, context.Response.StatusCode, stopwatch.ElapsedMilliseconds);
    }
});

app.UseExceptionHandler(errorApp => errorApp.Run(async context =>
{
    var error = context.Features.Get<IExceptionHandlerFeature>()?.Error;
    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
    await context.Response.WriteAsJsonAsync(new { error = error?.Message ?? "Unexpected server error" });
}));

app.UseCors();

var json = new JsonSerializerOptions(JsonSerializerDefaults.Web);

app.MapGet("/api/docs", (IVectorStore store) => store.GetDocuments());
app.MapGet("/api/docs/{id}/chunks", (string id, IVectorStore store) =>
    store.GetChunks(id) is { } document
        ? Results.Ok(document)
        : Results.NotFound(new { error = "Document not found" }));
app.MapDelete("/api/docs/{id}", (string id, IVectorStore store) => { store.Remove(id); return Results.NoContent(); });

app.MapPost("/api/docs", async (HttpRequest req, IEmbeddingClient embeddings, IVectorStore store, ITextChunker chunker) =>
{
    var file = (await req.ReadFormAsync()).Files.FirstOrDefault();
    if (file is null) return Results.BadRequest("No file");

    var extension = Path.GetExtension(file.FileName);
    if (!string.Equals(extension, ".txt", StringComparison.OrdinalIgnoreCase) &&
        !string.Equals(extension, ".md", StringComparison.OrdinalIgnoreCase))
        return Results.BadRequest("Only .txt and .md files are supported");

    using var reader = new StreamReader(file.OpenReadStream());
    var texts = chunker.Split(await reader.ReadToEndAsync());
    var vectors = new List<float[]>();
    foreach (var batch in texts.Chunk(20)) vectors.AddRange(await embeddings.EmbedAsync(batch));
    var newId = Guid.NewGuid().ToString("N")[..8];
    var id = store.Add(texts.Select((text, i) => new Chunk(newId, file.FileName, i, text, vectors[i])));
    return Results.Ok(new { id, name = file.FileName, chunks = texts.Count });
});

app.MapPost("/api/chat", async (ChatReq body, HttpResponse res, IRagPipeline pipeline, CancellationToken ct) =>
{
    res.ContentType = "text/event-stream";
    try
    {
        await foreach (var ev in pipeline.RunAsync(body.SessionId, body.Message, ct))
        {
            await res.WriteAsync($"data: {JsonSerializer.Serialize((object)ev, json)}\n\n", ct);
            await res.Body.FlushAsync(ct);
        }
    }
    catch (Exception e)
    {
        await res.WriteAsync($"data: {JsonSerializer.Serialize(new { type = "error", message = e.Message }, json)}\n\n", ct);
    }
});

app.MapGet("/", () => Results.Redirect("/health"));
app.MapHealthChecks("/health");

app.Run("http://localhost:5080");
record ChatReq(string SessionId, string Message);
