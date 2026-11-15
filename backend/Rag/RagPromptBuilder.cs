using RagLab.Llm;
using RagLab.Memory;

namespace RagLab.Rag;

public sealed class RagPromptBuilder : IPromptBuilder
{
    // KV-cache tip: keep this prefix byte-identical on every request so providers/servers can reuse cached prefix.
    // Dynamic stuff (retrieved context) goes at the END, in the last user message.
    const string SystemPrompt =
        "You are a helpful assistant that answers questions about the user's documents. " +
        "Use the provided context. Cite sources like [1], [2]. If the answer is not in the context, say so.";

    public IReadOnlyList<ChatMessage> Build(ConversationSession session, RetrievalResult retrieval, string question)
    {
        // static prefix -> memory -> history -> context+question
        var messages = new List<ChatMessage> { new("system", SystemPrompt) };

        if (session.Summary != "")
            messages.Add(new ChatMessage("system", "Conversation summary so far: " + session.Summary));

        messages.AddRange(session.Turns);
        messages.Add(new ChatMessage("user", $"Context:\n{retrieval.Context}\n\nQuestion: {question}"));

        return messages;
    }
}
