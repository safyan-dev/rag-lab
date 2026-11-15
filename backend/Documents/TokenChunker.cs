namespace RagLab.Documents;

public sealed class TokenChunker : ITextChunker
{
    const int CharsPerToken = 4;

    public IReadOnlyList<string> Split(string text, int minTokens = 300, int maxTokens = 500, int overlapTokens = 50)
    {
        if (minTokens <= 0) throw new ArgumentOutOfRangeException(nameof(minTokens));
        if (maxTokens < minTokens) throw new ArgumentOutOfRangeException(nameof(maxTokens));
        if (overlapTokens < 0 || overlapTokens >= maxTokens) throw new ArgumentOutOfRangeException(nameof(overlapTokens));

        var normalized = text.Replace("\r", "");
        var minChars = minTokens * CharsPerToken;
        var maxChars = maxTokens * CharsPerToken;
        var targetChars = (minChars + maxChars) / 2;
        var overlapChars = overlapTokens * CharsPerToken;

        var chunks = new List<string>();
        var start = 0;

        while (start < normalized.Length)
        {
            var remaining = normalized.Length - start;
            if (remaining <= maxChars)
            {
                var tail = normalized[start..].Trim();
                if (tail.Length == 0) break;

                if (chunks.Count > 0 && chunks[^1].Length + tail.Length + 2 <= maxChars)
                    chunks[^1] = chunks[^1] + "\n\n" + tail;
                else
                    chunks.Add(tail);
                break;
            }

            var minEnd = Math.Min(start + minChars, normalized.Length);
            var targetEnd = Math.Min(start + targetChars, normalized.Length);
            var maxEnd = Math.Min(start + maxChars, normalized.Length);
            var end = FindBoundary(normalized, minEnd, targetEnd, maxEnd);

            var chunk = normalized[start..end].Trim();
            if (chunk.Length > 0) chunks.Add(chunk);

            var nextStart = Math.Max(start + 1, end - overlapChars);
            while (nextStart < end && !char.IsWhiteSpace(normalized[nextStart - 1]))
                nextStart++;

            start = nextStart;
        }

        return chunks;
    }

    static int FindBoundary(string text, int minEnd, int targetEnd, int maxEnd)
    {
        // Prefer a paragraph break closest to the target size, staying within the min/max window.
        for (var i = Math.Min(targetEnd, maxEnd - 2); i >= minEnd; i--)
        {
            if (text[i] == '\n' && text[i + 1] == '\n') return i + 2;
        }

        for (var i = Math.Max(targetEnd + 1, minEnd); i <= maxEnd - 2; i++)
        {
            if (text[i] == '\n' && text[i + 1] == '\n') return i + 2;
        }

        // Otherwise split at the last word boundary before maxChars.
        for (var i = maxEnd - 1; i >= minEnd; i--)
        {
            if (char.IsWhiteSpace(text[i])) return i;
        }

        return maxEnd;
    }
}
