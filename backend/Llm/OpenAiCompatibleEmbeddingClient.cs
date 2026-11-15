using System.Text.Json;
using Microsoft.Extensions.Options;
using RagLab.Configuration;

namespace RagLab.Llm;

public sealed class OpenAiCompatibleEmbeddingClient(HttpClient http, IOptions<LlmOptions> options)
    : OpenAiCompatibleClient(http, options), IEmbeddingClient
{
    public async Task<float[][]> EmbedAsync(IReadOnlyList<string> texts, CancellationToken cancellationToken = default)
    {
        using var response = await PostAsync(
            "embeddings",
            new { model = Options.EmbeddingModel, input = texts, dimensions = Options.EmbeddingDimensions },
            HttpCompletionOption.ResponseContentRead,
            cancellationToken);

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
        return document.RootElement.GetProperty("data").EnumerateArray()
            .Select(item => item.GetProperty("embedding").EnumerateArray().Select(value => value.GetSingle()).ToArray())
            .ToArray();
    }
}
