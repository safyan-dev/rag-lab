namespace RagLab.Memory;

public interface IConversationCompactor
{
    Task CompactAsync(ConversationSession session, CancellationToken cancellationToken = default);
}
