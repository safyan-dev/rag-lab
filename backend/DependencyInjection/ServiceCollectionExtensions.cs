using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pgvector.EntityFrameworkCore;
using RagLab.Configuration;
using RagLab.Data;
using RagLab.Documents;
using RagLab.Llm;
using RagLab.Memory;
using RagLab.Rag;
using RagLab.Search;

namespace RagLab.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRagLab(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<LlmOptions>(configuration.GetSection(LlmOptions.SectionName));

        var postgresConnection = configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException("Connection string 'Postgres' is not configured.");

        services.AddDbContextFactory<RagDbContext>(options =>
            options.UseNpgsql(postgresConnection, npgsql => npgsql.UseVector()));
        services.AddHostedService<DatabaseInitializer>();

        services.AddHttpClient<IChatClient, OpenAiCompatibleChatClient>();
        services.AddHttpClient<IEmbeddingClient, OpenAiCompatibleEmbeddingClient>();

        services.AddSingleton<ITextChunker, TokenChunker>();
        services.AddSingleton<IVectorStore, EfVectorStore>();
        services.AddSingleton<ISessionStore, ConversationMemory>();
        services.AddSingleton<IPromptBuilder, RagPromptBuilder>();

        services.AddTransient<IConversationCompactor, LlmConversationCompactor>();
        services.AddTransient<IRetriever, DocumentRetriever>();
        services.AddTransient<IRagPipeline, RagPipeline>();

        return services;
    }
}
