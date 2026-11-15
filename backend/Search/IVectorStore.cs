using RagLab.Documents;

namespace RagLab.Search;

public interface IVectorStore
{
    string Add(IEnumerable<Chunk> chunks);

    IReadOnlyList<DocumentInfo> GetDocuments();

    DocumentChunks? GetChunks(string docId);

    void Remove(string docId);

    IReadOnlyList<RetrievedChunk> Search(float[] query, int k = 4);
}
