namespace RagLab.Data;

public sealed class DocumentEntity
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public DateTime UploadedAt { get; set; }
    public List<ChunkEntity> Chunks { get; set; } = [];
}
