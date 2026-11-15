namespace RagLab.Rag;

public interface IRetriever
{
    Task<RetrievalResult> RetrieveAsync(string question, CancellationToken cancellationToken = default);
}
