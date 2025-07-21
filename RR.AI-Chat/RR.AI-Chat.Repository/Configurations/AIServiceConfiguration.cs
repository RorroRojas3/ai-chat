using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RR.AI_Chat.Dto.Enums;
using RR.AI_Chat.Entity;

namespace RR.AI_Chat.Repository.Configurations
{
    public class AIServiceConfiguration : IEntityTypeConfiguration<Entity.AIService>
    {
        public void Configure(EntityTypeBuilder<Entity.AIService> builder)
        {
            var date = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            builder.HasData(
                new AIService
                {
                    Id = AIServiceType.Ollama,
                    Name = nameof(AIServiceType.Ollama),
                    DateCreated = date,
                },
                new AIService
                {
                    Id = AIServiceType.OpenAI,
                    Name = nameof(AIServiceType.OpenAI),
                    DateCreated = date,
                },
                new AIService
                {
                    Id = AIServiceType.AzureOpenAI,
                    Name = nameof(AIServiceType.AzureOpenAI),
                    DateCreated = date,
                }
            );
        }
    }
}
