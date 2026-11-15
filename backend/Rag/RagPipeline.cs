using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using RagLab.Llm;
using RagLab.Memory;

namespace RagLab.Rag;

public sealed class RagPipeline(
    IRetriever retriever,
    IPromptBuilder promptBuilder,
    ISessionStore sessions,
    IConversationCompactor compactor,
    IChatClient chatClient,
    ILogger<RagPipeline> logger) : IRagPipeline
{
    public async IAsyncEnumerable<RagEvent> RunAsync(
        string sessionId,
        string question,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var session = sessions.Get(sessionId);

        // 1) RETRIEVE
        var retrieval = await retriever.RetrieveAsync(question, cancellationToken);
        var retrievalMs = stopwatch.ElapsedMilliseconds;

        // 2) ASSEMBLE PROMPT
        var messages = promptBuilder.Build(session, retrieval, question);
        var promptChars = JsonSerializer.Serialize(messages).Length;
        var approxTokens = promptChars / 4;

        logger.LogInformation(
            "Chat prompt {SessionId}: {PromptChars} chars (~{ApproxTokens} tokens), {MemoryTurns} memory turns, {SourceCount} sources, hasSummary={HasSummary}",
            sessionId, promptChars, approxTokens, session.TurnCount, retrieval.Sources.Count, session.Summary != "");

        yield return new TraceEvent(
            retrievalMs,
            promptChars,
            approxTokens,
            session.TurnCount,
            session.Summary != "",
            retrieval.Sources
                .Select(hit => new SourceReference(hit.Chunk.DocName, Math.Round(hit.Score, 3), hit.Chunk.Text))
                .ToList());

        // 3) GENERATE (stream)
        var answer = new StringBuilder();
        long? ttft = null;
        ChatUsage? usage = null;

        await foreach (var chunk in chatClient.StreamAsync(messages, cancellationToken))
        {
            if (chunk.Usage is not null) usage = chunk.Usage;
            if (chunk.Content is not { Length: > 0 } token) continue;

            ttft ??= stopwatch.ElapsedMilliseconds;
            answer.Append(token);
            yield return new TokenEvent(token);
        }

        if (usage is not null)
            logger.LogInformation(
                "Chat usage {SessionId}: prompt={PromptTokens}, completion={CompletionTokens}, total={TotalTokens}",
                sessionId, usage.PromptTokens, usage.CompletionTokens, usage.TotalTokens);

        // 4) UPDATE MEMORY (store the raw question, not the context-stuffed prompt)
        session.AddTurn("user", question);
        session.AddTurn("assistant", answer.ToString());
        await compactor.CompactAsync(session, cancellationToken);

        yield return new DoneEvent(ttft, stopwatch.ElapsedMilliseconds, usage?.PromptTokens, usage?.CompletionTokens);
    }
}
