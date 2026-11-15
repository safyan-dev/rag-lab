namespace RagLab.Rag;

public interface IRagPipeline
{
    IAsyncEnumerable<RagEvent> RunAsync(string sessionId, string question, CancellationToken cancellationToken = default);
}
