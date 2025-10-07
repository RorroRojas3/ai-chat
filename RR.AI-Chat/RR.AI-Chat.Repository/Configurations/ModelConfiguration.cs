using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RR.AI_Chat.Dto.Enums;
using RR.AI_Chat.Entity;

namespace RR.AI_Chat.Repository.Configurations
{
    public class ModelConfiguration : IEntityTypeConfiguration<Model>
    {
        public void Configure(EntityTypeBuilder<Model> builder)
        {
            var dateCreated = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            builder.HasData(
                new Model
                {
                    Id = new("c36e22ed-262a-47a1-b2ba-06a38355ae0f"),
                    AIServiceId = AIServiceType.AzureAIFoundry,
                    Name = "gpt-5-mini",
                    Encoding = "o200k_harmony",
                    IsToolEnabled = true,
                    DateCreated = dateCreated
                },
                new Model
                {
                    Id = new("fd01b615-1e9f-46af-957f-e4eaeff02766"),
                    AIServiceId = AIServiceType.AzureAIFoundry,
                    Name = "gpt-5-nano",
                    Encoding = "o200k_harmony",
                    IsToolEnabled = true,
                    DateCreated = dateCreated
                },
                new Model
                {
                    Id = new("0b3948f5-70df-4697-a033-ae70971e1796"),
                    AIServiceId = AIServiceType.AzureAIFoundry,
                    Name = "gpt-5-chat",
                    Encoding = "o200k_harmony",
                    IsToolEnabled = true,
                    DateCreated = dateCreated
                }
            );
        }
    }
}
