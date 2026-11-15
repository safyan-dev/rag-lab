using System.Runtime.CompilerServices;
using System.Text.Json;
using Microsoft.Extensions.Options;
using RagLab.Configuration;

namespace RagLab.Llm;

public sealed class OpenAiCompatibleChatClient(HttpClient http, IOptions<LlmOptions> options)
    : OpenAiCompatibleClient(http, options), IChatClient
{
    public async Task<string> CompleteAsync(IReadOnlyList<ChatMessage> messages, CancellationToken cancellationToken = default)
    {
        using var response = await PostAsync(
            "chat/completions",
            new { model = Options.ChatModel, messages, temperature = Options.Temperature },
            HttpCompletionOption.ResponseContentRead,
            cancellationToken);

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
        return document.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString() ?? "";
    }

    public async IAsyncEnumerable<ChatStreamChunk> StreamAsync(
        IReadOnlyList<ChatMessage> messages,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var response = await PostAsync(
            "chat/completions",
            new
            {
                model = Options.ChatModel,
                messages,
                temperature = Options.Temperature,
                stream = true,
                stream_options = new { include_usage = true }
            },
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        using var reader = new StreamReader(await response.Content.ReadAsStreamAsync(cancellationToken));
        string? line;
        while ((line = await reader.ReadLineAsync(cancellationToken)) != null)
        {
            if (!line.StartsWith("data: ")) continue;

            var data = line[6..];
            if (data == "[DONE]") yield break;

            using var document = JsonDocument.Parse(data);
            var root = document.RootElement;

            string? token = null;
            if (root.TryGetProperty("choices", out var choices)
                && choices.ValueKind == JsonValueKind.Array
                && choices.GetArrayLength() > 0
                && choices[0].TryGetProperty("delta", out var delta)
                && delta.TryGetProperty("content", out var content)
                && content.GetString() is { Length: > 0 } text)
                token = text;

            ChatUsage? usage = null;
            if (root.TryGetProperty("usage", out var usageElement) && usageElement.ValueKind == JsonValueKind.Object)
            {
                usage = new ChatUsage(
                    usageElement.TryGetProperty("prompt_tokens", out var promptTokens) ? promptTokens.GetInt32() : 0,
                    usageElement.TryGetProperty("completion_tokens", out var completionTokens) ? completionTokens.GetInt32() : 0,
                    usageElement.TryGetProperty("total_tokens", out var totalTokens) ? totalTokens.GetInt32() : 0);
            }

            if (token is not null || usage is not null)
                yield return new ChatStreamChunk(token, usage);
        }
    }
}
