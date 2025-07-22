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
                    Id = new("157b91cf-1880-4977-9b7a-7f80f548df04"),
                    AIServiceId = AIServiceType.Ollama,
                    Name = "llama3.2",
                    IsToolEnabled = false,
                    DateCreated = dateCreated
                },
                new Model
                {
                    Id = new("9910ba5f-faca-4790-88a4-352e71e14724"),
                    AIServiceId = AIServiceType.Ollama,
                    Name = "mistral",
                    IsToolEnabled = false,
                    DateCreated = dateCreated
                },
                new Model
                {
                    Id = new("1fe5381b-0262-469a-b63e-f4d0c4807a98"),
                    AIServiceId = AIServiceType.Ollama,
                    Name = "gemma3",
                    IsToolEnabled = false,
                    DateCreated = dateCreated
                },
                new Model
                {
                    Id = new("1983e31e-627d-4617-9320-17ded79efa2b"),
                    AIServiceId = AIServiceType.OpenAI,
                    Name = "gpt-4.1-nano",
                    IsToolEnabled = true,
                    DateCreated = dateCreated
                },
                new Model
                {
                    Id = new("e9bc0791-2d15-43c8-9299-5c86039786f9"),
                    AIServiceId = AIServiceType.OpenAI,
                    Name = "gpt-4.1-mini",
                    IsToolEnabled = true,
                    DateCreated = dateCreated
                },
                new Model
                {
                    Id = new("a24fcce0-02e7-4ecb-88d7-27f33e47fecf"),
                    AIServiceId = AIServiceType.AzureOpenAI,
                    Name = "gpt-4.1-nano",
                    IsToolEnabled = true,
                    DateCreated = dateCreated
                }
            );
        }
    }
}
