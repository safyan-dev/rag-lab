namespace RagLab.Memory;

public interface ISessionStore
{
    ConversationSession Get(string sessionId);
}
