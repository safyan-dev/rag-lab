using Pgvector;

namespace RagLab.Data;

public sealed class ChunkEntity
{
    public long Id { get; set; }
    public string DocumentId { get; set; } = "";
    public DocumentEntity Document { get; set; } = null!;
    public int ChunkIndex { get; set; }
    public string Text { get; set; } = "";
    public Vector Embedding { get; set; } = null!;
}
