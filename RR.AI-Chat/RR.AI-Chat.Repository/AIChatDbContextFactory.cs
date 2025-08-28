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
                ?? "Server=sqlserver,1433;Database=aichat;User Id=sa;Password=DevPassword123!;TrustServerCertificate=true;";
            
            optionsBuilder.UseSqlServer(connectionString, o => o.UseVectorSearch());
            
            return new AIChatDbContext(optionsBuilder.Options);
        }
    }
}
