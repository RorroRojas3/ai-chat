using Microsoft.EntityFrameworkCore;
using RR.AI_Chat.Entity;
using RR.AI_Chat.Repository.Configurations;

namespace RR.AI_Chat.Repository
{
    public class AIChatDbContext(DbContextOptions<AIChatDbContext> options) : DbContext(options)
    {
        #region DbSets
        public virtual DbSet<AIService> AIServices { get; set; }

        public virtual DbSet<Document> Documents { get; set; }

        public virtual DbSet<DocumentPage> DocumentPages { get; set; }

        public virtual DbSet<Model> Models { get; set; }

        public virtual DbSet<Session> Sessions { get; set; }

        public virtual DbSet<SessionDetail> SessionDetails { get; set; }
        #endregion

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Enable vector extension for PostgreSQL
            modelBuilder.HasPostgresExtension("vector");

            modelBuilder.Entity<DocumentPage>()
               .HasIndex(i => i.Embedding)
               .HasMethod("hnsw")
               .HasOperators("vector_cosine_ops")
               .HasStorageParameter("m", 16)
               .HasStorageParameter("ef_construction", 64);

            modelBuilder.ApplyConfiguration(new AIServiceConfiguration());
            modelBuilder.ApplyConfiguration(new ModelConfiguration());
        }
    }
}
