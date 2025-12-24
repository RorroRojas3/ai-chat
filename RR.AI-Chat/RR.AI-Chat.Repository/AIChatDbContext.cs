using Microsoft.EntityFrameworkCore;
using RR.AI_Chat.Entity;
using RR.AI_Chat.Repository.Configurations;

namespace RR.AI_Chat.Repository
{
    public class AIChatDbContext(DbContextOptions<AIChatDbContext> options) : DbContext(options)
    {
        #region DbSets
        public DbSet<AIService> AIServices { get; set; }

        public DbSet<Model> Models { get; set; }

        public DbSet<Session> Sessions { get; set; }

        public DbSet<SessionDocument> SessionDocuments { get; set; }

        public DbSet<SessionDocumentPage> SessionDocumentPages { get; set; }

        public DbSet<Project> Projects { get; set; }

        public DbSet<ProjectDocument> ProjectDocuments { get; set; }

        public DbSet<ProjectDocumentPage> ProjectDocumentPages { get; set; }

        public DbSet<User> Users { get; set; }
        #endregion

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Session>(entity =>
            {
                entity.ComplexProperty(x => x.Chat, x => x.ToJson());

            });
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
