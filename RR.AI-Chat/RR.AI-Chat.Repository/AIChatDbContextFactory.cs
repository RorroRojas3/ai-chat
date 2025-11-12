using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace RR.AI_Chat.Repository
{
    public class AIChatDbContextFactory : IDesignTimeDbContextFactory<AIChatDbContext>
    {
        public AIChatDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AIChatDbContext>();
            
            // Get connection string from environment variable or use default
            var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection") 
                ?? "Server=localhost;Database=aichat;Integrated Security=true;TrustServerCertificate=true;";
            
            optionsBuilder.UseSqlServer(connectionString);
            
            return new AIChatDbContext(optionsBuilder.Options);
        }
    }
}
