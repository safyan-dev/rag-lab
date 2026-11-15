namespace RagLab.Configuration;

public sealed class LlmOptions
{
    public const string SectionName = "Llm";

    public string BaseUrl { get; set; } = "";
    public string ApiKey { get; set; } = "";
    public string ChatModel { get; set; } = "";
    public double Temperature { get; set; } = 0.2;
    public string EmbeddingModel { get; set; } = "";
    public int EmbeddingDimensions { get; set; } = 768;
}
