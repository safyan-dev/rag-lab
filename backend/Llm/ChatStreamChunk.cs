namespace RagLab.Llm;

public sealed record ChatUsage(int PromptTokens, int CompletionTokens, int TotalTokens);

public sealed record ChatStreamChunk(string? Content, ChatUsage? Usage);
