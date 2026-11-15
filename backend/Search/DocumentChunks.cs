namespace RagLab.Search;

public sealed record ChunkInfo(int Index, string Text);

public sealed record DocumentChunks(string Id, string Name, DateTime UploadedAt, IReadOnlyList<ChunkInfo> Chunks)
{
    public int TotalChunks => Chunks.Count;
}
