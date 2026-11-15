using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using RagLab.Configuration;

namespace RagLab.Llm;

public abstract class OpenAiCompatibleClient(HttpClient http, IOptions<LlmOptions> options)
{
    protected LlmOptions Options => options.Value;

    protected async Task<HttpResponseMessage> PostAsync(
        string path,
        object body,
        HttpCompletionOption completion,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, $"{Options.BaseUrl.TrimEnd('/')}/{path}")
        {
            Content = JsonContent.Create(body)
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", Options.ApiKey);

        var response = await http.SendAsync(request, completion, cancellationToken);
        response.EnsureSuccessStatusCode();
        return response;
    }
}
