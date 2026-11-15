using RagLab.Search;

namespace RagLab.Rag;

public sealed record RetrievalResult(string Context, IReadOnlyList<RetrievedChunk> Sources);
