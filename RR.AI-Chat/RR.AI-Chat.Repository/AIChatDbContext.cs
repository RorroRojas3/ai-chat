using Microsoft.EntityFrameworkCore;
using RR.AI_Chat.Entity;
using RR.AI_Chat.Repository.Configurations;
using System.Text.Json;

namespace RR.AI_Chat.Repository
{
    public class AIChatDbContext(DbContextOptions<AIChatDbContext> options) : DbContext(options)
    {
        #region DbSets
        public DbSet<AIService> AIServices { get; set; }

        public DbSet<Document> Documents { get; set; }

        public DbSet<DocumentPage> DocumentPages { get; set; }

        public DbSet<Model> Models { get; set; }

        public DbSet<Session> Sessions { get; set; }

        public DbSet<Project> Projects { get; set; }

        public DbSet<User> Users { get; set; }
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

            modelBuilder.ApplyConfiguration(new AIServiceConfiguration());
            modelBuilder.ApplyConfiguration(new ModelConfiguration());

            // Configure global delete behavior
            foreach (var relationship in modelBuilder.Model.GetEntityTypes()
                .SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.NoAction;
            }
        }
    }
}
