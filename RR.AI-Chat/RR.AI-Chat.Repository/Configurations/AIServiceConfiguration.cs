using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RR.AI_Chat.Dto.Enums;
using RR.AI_Chat.Entity;

namespace RR.AI_Chat.Repository.Configurations
{
    public class AIServiceConfiguration : IEntityTypeConfiguration<AIService>
    {
        public void Configure(EntityTypeBuilder<AIService> builder)
        {
            builder.HasData(
                new AIService
                {
                    Id = AIServiceType.Ollama,
                    Name = "Ollama",
                    DateCreated = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                }
            );
        }
    }
}
