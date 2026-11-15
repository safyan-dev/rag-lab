using RagLab.Llm;
using RagLab.Memory;

namespace RagLab.Rag;

public interface IPromptBuilder
{
    IReadOnlyList<ChatMessage> Build(ConversationSession session, RetrievalResult retrieval, string question);
}
