namespace RagLab.Rag;

public abstract record RagEvent(string Type);

public sealed record TraceEvent(
    long RetrievalMs,
    int PromptChars,
    int ApproxTokens,
    int MemoryTurns,
    bool HasSummary,
    IReadOnlyList<SourceReference> Sources) : RagEvent("trace");

public sealed record TokenEvent(string Text) : RagEvent("token");

public sealed record DoneEvent(long? TtftMs, long TotalMs, int? PromptTokens, int? CompletionTokens) : RagEvent("done");

public sealed record SourceReference(string Doc, double Score, string Text);
