namespace RagLab.Llm;

public interface IChatClient
{
    Task<string> CompleteAsync(IReadOnlyList<ChatMessage> messages, CancellationToken cancellationToken = default);

    IAsyncEnumerable<ChatStreamChunk> StreamAsync(IReadOnlyList<ChatMessage> messages, CancellationToken cancellationToken = default);
}
