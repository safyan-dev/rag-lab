using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Pgvector.EntityFrameworkCore;
using RagLab.Configuration;

namespace RagLab.Data;

public sealed class RagDbContext(
    DbContextOptions<RagDbContext> options,
    IOptions<LlmOptions> llmOptions) : DbContext(options)
{
    public DbSet<DocumentEntity> Documents => Set<DocumentEntity>();
    public DbSet<ChunkEntity> Chunks => Set<ChunkEntity>();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var dimensions = llmOptions.Value.EmbeddingDimensions;

        modelBuilder.HasPostgresExtension("vector");

        modelBuilder.Entity<DocumentEntity>(entity =>
        {
            entity.ToTable("documents", table =>
                table.HasCheckConstraint("documents_supported_extension", "name ~* '\\.(txt|md)$'"));
            entity.HasKey(document => document.Id);
            entity.Property(document => document.Id).HasColumnName("id");
            entity.Property(document => document.Name).HasColumnName("name").IsRequired();
            entity.Property(document => document.UploadedAt).HasColumnName("uploaded_at").HasDefaultValueSql("now()");
        });

        modelBuilder.Entity<ChunkEntity>(entity =>
        {
            entity.ToTable("chunks");
            entity.HasKey(chunk => chunk.Id);
            entity.Property(chunk => chunk.Id).HasColumnName("id");
            entity.Property(chunk => chunk.DocumentId).HasColumnName("document_id").IsRequired();
            entity.Property(chunk => chunk.ChunkIndex).HasColumnName("chunk_index").IsRequired();
            entity.Property(chunk => chunk.Text).HasColumnName("text").IsRequired();
            entity.Property(chunk => chunk.Embedding).HasColumnName("embedding").HasColumnType($"vector({dimensions})").IsRequired();

            entity.HasIndex(chunk => new { chunk.DocumentId, chunk.ChunkIndex }).IsUnique();
            entity.HasOne(chunk => chunk.Document)
                .WithMany(document => document.Chunks)
                .HasForeignKey(chunk => chunk.DocumentId)
                .OnDelete(DeleteBehavior.Cascade);

            // pgvector's HNSW index supports at most 2,000 dimensions for the vector type.
            if (dimensions <= 2000)
                entity.HasIndex(chunk => chunk.Embedding).HasMethod("hnsw").HasOperators("vector_cosine_ops");
        });
    }
}
