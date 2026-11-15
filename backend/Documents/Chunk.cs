namespace RagLab.Documents;

public sealed record Chunk(string DocId, string DocName, int ChunkIndex, string Text, float[] Vec);
