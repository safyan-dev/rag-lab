using Microsoft.EntityFrameworkCore;

namespace RagLab.Data;

public sealed class DatabaseInitializer(IDbContextFactory<RagDbContext> dbFactory) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);
        await db.Database.ExecuteSqlRawAsync("CREATE EXTENSION IF NOT EXISTS vector", cancellationToken);
        await db.Database.EnsureCreatedAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
