using RagLab.Llm;

namespace RagLab.Memory;

public sealed class LlmConversationCompactor(IChatClient chatClient) : IConversationCompactor
{
    const int MaxTurns = 8; // when exceeded, oldest 4 turns get folded into the summary
    const int TurnsToFold = 4;

    public async Task CompactAsync(ConversationSession session, CancellationToken cancellationToken = default)
    {
        if (session.TurnCount <= MaxTurns) return;

        var oldTurns = session.TakeOldest(TurnsToFold);
        var transcript = string.Join("\n", oldTurns.Select(turn => $"{turn.Role}: {turn.Content}"));

        session.Summary = await chatClient.CompleteAsync(
        [
            new ChatMessage("system", "Merge the previous summary and new turns into a concise summary (max 120 words). Keep facts, names, decisions."),
            new ChatMessage("user", $"Previous summary:\n{session.Summary}\n\nNew turns:\n{transcript}")
        ], cancellationToken);
    }
}
