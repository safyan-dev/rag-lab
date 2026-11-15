namespace RagLab.Memory;

public sealed class ConversationMemory : ISessionStore
{
    readonly Dictionary<string, ConversationSession> sessions = [];

    public ConversationSession Get(string sessionId)
    {
        lock (sessions)
        {
            return sessions.TryGetValue(sessionId, out var session)
                ? session
                : sessions[sessionId] = new ConversationSession();
        }
    }
}
