using Microsoft.EntityFrameworkCore;
using RR.AI_Chat.Entity;

namespace RR.AI_Chat.Repository
{
    public class AIChatDbContext(DbContextOptions<AIChatDbContext> options) : DbContext(options)
    {
        #region DbSets
        public virtual DbSet<Session> Sessions { get; set; }

        public virtual DbSet<Document> Documents { get; set; }

        public virtual DbSet<DocumentPage> DocumentPages { get; set; }
        #endregion
    }
}
