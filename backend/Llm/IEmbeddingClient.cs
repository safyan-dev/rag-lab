namespace RagLab.Llm;

public interface IEmbeddingClient
{
    Task<float[][]> EmbedAsync(IReadOnlyList<string> texts, CancellationToken cancellationToken = default);
}
