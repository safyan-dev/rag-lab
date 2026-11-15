namespace RagLab.Documents;

public interface ITextChunker
{
    IReadOnlyList<string> Split(string text, int minTokens = 300, int maxTokens = 500, int overlapTokens = 50);
}
