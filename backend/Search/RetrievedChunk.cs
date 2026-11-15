using RagLab.Documents;

namespace RagLab.Search;

public sealed record RetrievedChunk(Chunk Chunk, float Score);
