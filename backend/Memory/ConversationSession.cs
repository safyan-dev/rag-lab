using RagLab.Llm;

namespace RagLab.Memory;

public sealed class ConversationSession
{
    readonly List<ChatMessage> turns = [];

    public IReadOnlyList<ChatMessage> Turns => turns;
    public string Summary { get; set; } = "";
    public int TurnCount => turns.Count;

    public void AddTurn(string role, string content) => turns.Add(new ChatMessage(role, content));

    public IReadOnlyList<ChatMessage> TakeOldest(int count)
    {
        var oldest = turns.Take(count).ToList();
        turns.RemoveRange(0, oldest.Count);
        return oldest;
    }
}
