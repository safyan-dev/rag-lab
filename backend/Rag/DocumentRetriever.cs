using RagLab.Llm;
using RagLab.Search;

namespace RagLab.Rag;

public sealed class DocumentRetriever(IEmbeddingClient embeddings, IVectorStore store) : IRetriever
{
    public async Task<RetrievalResult> RetrieveAsync(string question, CancellationToken cancellationToken = default)
    {
        var queryVector = (await embeddings.EmbedAsync([question], cancellationToken))[0];
        var hits = store.Search(queryVector);
        var context = string.Join("\n\n", hits.Select((hit, i) => $"[{i + 1}] ({hit.Chunk.DocName})\n{hit.Chunk.Text}"));

        return new RetrievalResult(context, hits);
    }
}
