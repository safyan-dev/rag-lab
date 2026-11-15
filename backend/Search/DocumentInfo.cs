namespace RagLab.Search;

public sealed record DocumentInfo(string Id, string Name, int Chunks, DateTime UploadedAt = default);
