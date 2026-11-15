using System.Text.Json.Serialization;

namespace RagLab.Llm;

public sealed record ChatMessage(
    [property: JsonPropertyName("role")] string Role,
    [property: JsonPropertyName("content")] string Content);
