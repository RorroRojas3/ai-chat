using Microsoft.EntityFrameworkCore;
using RR.AI_Chat.Entity;
using RR.AI_Chat.Repository.Configurations;
using System.Text.Json;

namespace RR.AI_Chat.Repository
{
    public class AIChatDbContext(DbContextOptions<AIChatDbContext> options) : DbContext(options)
    {
        #region DbSets
        public virtual DbSet<AIService> AIServices { get; set; }

        public virtual DbSet<Document> Documents { get; set; }

        public virtual DbSet<DocumentPage> DocumentPages { get; set; }

        public virtual DbSet<Model> Models { get; set; }

        public virtual DbSet<McpServer> McpServers { get; set; }

        public virtual DbSet<Session> Sessions { get; set; }

        public virtual DbSet<User> Users { get; set; }
        #endregion

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Session>(entity =>
            {
                // EF Core will handle ChatMessage serialization automatically
                entity.Property(e => e.Conversations)
                      .HasColumnType("nvarchar(max)") 
                      .HasConversion(
                      v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                      v => JsonSerializer.Deserialize<List<Conversation>>(v, (JsonSerializerOptions?)null) ?? new());

            });

            modelBuilder.Entity<DocumentPage>().Property(p => p.Embedding).HasColumnType("vector(1536)");

            modelBuilder.Entity<McpServer>(e =>
            {
                e.Property(p => p.Arguments)
                    .HasColumnType("nvarchar(max)")
                    .HasConversion(
                        v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                        v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new());
            });

            modelBuilder.ApplyConfiguration(new AIServiceConfiguration());
            modelBuilder.ApplyConfiguration(new ModelConfiguration());
            modelBuilder.ApplyConfiguration(new McpServerConfiguration());
        }
    }
}
