using Microsoft.EntityFrameworkCore;
using Pgvector;
using Pgvector.EntityFrameworkCore;
using RagLab.Data;
using RagLab.Documents;

namespace RagLab.Search;

public sealed class EfVectorStore(IDbContextFactory<RagDbContext> dbFactory) : IVectorStore
{
    public string Add(IEnumerable<Chunk> chunks)
    {
        var items = chunks.ToList();
        if (items.Count == 0) return "";

        using var db = dbFactory.CreateDbContext();
        using var transaction = db.Database.BeginTransaction();

        var name = items[0].DocName;
        var document = db.Documents.FirstOrDefault(existing => existing.Name == name);
        if (document is null)
        {
            document = new DocumentEntity
            {
                Id = items[0].DocId,
                Name = name,
                UploadedAt = DateTime.UtcNow
            };
            db.Documents.Add(document);
        }
        else
        {
            document.UploadedAt = DateTime.UtcNow;
            db.Chunks.RemoveRange(db.Chunks.Where(chunk => chunk.DocumentId == document.Id));
            db.SaveChanges();
        }

        db.Chunks.AddRange(items.Select(chunk => new ChunkEntity
        {
            DocumentId = document.Id,
            ChunkIndex = chunk.ChunkIndex,
            Text = chunk.Text,
            Embedding = new Vector(chunk.Vec)
        }));

        db.SaveChanges();
        transaction.Commit();
        return document.Id;
    }

    public IReadOnlyList<DocumentInfo> GetDocuments()
    {
        using var db = dbFactory.CreateDbContext();
        return db.Documents
            .AsNoTracking()
            .OrderByDescending(document => document.UploadedAt)
            .Select(document => new DocumentInfo(
                document.Id,
                document.Name,
                document.Chunks.Count,
                document.UploadedAt))
            .ToList();
    }

    public DocumentChunks? GetChunks(string docId)
    {
        using var db = dbFactory.CreateDbContext();
        var document = db.Documents
            .AsNoTracking()
            .Where(existing => existing.Id == docId)
            .Select(existing => new { existing.Id, existing.Name, existing.UploadedAt })
            .FirstOrDefault();
        if (document is null) return null;

        var chunks = db.Chunks
            .AsNoTracking()
            .Where(chunk => chunk.DocumentId == docId)
            .OrderBy(chunk => chunk.ChunkIndex)
            .Select(chunk => new { chunk.ChunkIndex, chunk.Text })
            .ToList()
            .Select(chunk => new ChunkInfo(chunk.ChunkIndex, chunk.Text))
            .ToList();

        return new DocumentChunks(document.Id, document.Name, document.UploadedAt, chunks);
    }

    public void Remove(string docId)
    {
        using var db = dbFactory.CreateDbContext();
        var document = db.Documents.Find(docId);
        if (document is null) return;

        db.Documents.Remove(document);
        db.SaveChanges();
    }

    public IReadOnlyList<RetrievedChunk> Search(float[] query, int k = 4)
    {
        using var db = dbFactory.CreateDbContext();
        var vector = new Vector(query);

        var rows = db.Chunks
            .AsNoTracking()
            .Select(chunk => new
            {
                chunk.DocumentId,
                DocumentName = chunk.Document.Name,
                chunk.ChunkIndex,
                chunk.Text,
                chunk.Embedding,
                Distance = chunk.Embedding.CosineDistance(vector)
            })
            .OrderBy(row => row.Distance)
            .Take(k)
            .ToList();

        return rows
            .Select(row => new RetrievedChunk(
                new Chunk(row.DocumentId, row.DocumentName, row.ChunkIndex, row.Text, row.Embedding.ToArray()),
                (float)(1 - row.Distance)))
            .ToList();
    }
}
