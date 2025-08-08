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
                    Id = new("0b1169ca-f92a-4e3c-9441-6e89efc66424"),
                    AIServiceId = AIServiceType.OpenAI,
                    Name = "gpt-5-nano",
                    IsToolEnabled = true,
                    DateCreated = dateCreated
                },
                new Model
                {
                    Id = new("ebcfd808-9e43-4fe4-a88d-e09b397e05a6"),
                    AIServiceId = AIServiceType.OpenAI,
                    Name = "gpt-5-mini",
                    IsToolEnabled = true,
                    DateCreated = dateCreated
                },
                new Model
                {
                    Id = new("868283a7-8ba2-4807-80e8-67b801c3417e"),
                    AIServiceId = AIServiceType.OpenAI,
                    Name = "gpt-5",
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
                    AIServiceId = AIServiceType.AzureAIFoundry,
                    Name = "gpt-4.1-nano",
                    IsToolEnabled = true,
                    DateCreated = dateCreated
                },
                new Model
                {
                    Id = new("e2034bfc-5ae5-48c3-a140-5bc8386ede41"),
                    AIServiceId = AIServiceType.AzureAIFoundry,
                    Name = "gpt-4.1-mini",
                    IsToolEnabled = true,
                    DateCreated = dateCreated
                },
                new Model
                {
                    Id = new("c36e22ed-262a-47a1-b2ba-06a38355ae0f"),
                    AIServiceId = AIServiceType.AzureAIFoundry,
                    Name = "gpt-5-mini",
                    IsToolEnabled = true,
                    DateCreated = dateCreated
                },
                new Model
                {
                    Id = new("fd01b615-1e9f-46af-957f-e4eaeff02766"),
                    AIServiceId = AIServiceType.AzureAIFoundry,
                    Name = "gpt-5-nano",
                    IsToolEnabled = true,
                    DateCreated = dateCreated
                },
                new Model
                {
                    Id = new("2f461194-2932-4185-bc69-5f9ae69effbc"),
                    AIServiceId = AIServiceType.AzureAIFoundry,
                    Name = "DeepSeek-V3-0324",
                    IsToolEnabled = true,
                    DateCreated = dateCreated
                },
                new Model
                {
                    Id = new("f35b51d7-c8d3-4040-8bff-8de67b4d3c25"),
                    AIServiceId = AIServiceType.AzureAIFoundry,
                    Name = "grok-3-mini",
                    IsToolEnabled = true,
                    DateCreated = dateCreated
                },
                new Model
                {
                    Id = new("98591f36-58b1-4941-834e-0aa09f9f4243"),
                    AIServiceId = AIServiceType.AzureAIFoundry,
                    Name = "grok-3",
                    IsToolEnabled = true,
                    DateCreated = dateCreated
                },
                new Model
                {
                    Id = new("169b6b77-4949-442e-b27f-a7bfb1cd3370"),
                    AIServiceId = AIServiceType.AzureAIFoundry,
                    Name = "gpt-5-chat",
                    IsToolEnabled = true,
                    DateCreated = dateCreated
                }
            );
        }
    }
}
