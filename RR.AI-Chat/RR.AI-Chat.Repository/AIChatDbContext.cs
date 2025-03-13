using Microsoft.EntityFrameworkCore;

namespace RR.AI_Chat.Repository
{
    public class AIChatDbContext(DbContextOptions<AIChatDbContext> options) : DbContext(options)
    {
    }
}
