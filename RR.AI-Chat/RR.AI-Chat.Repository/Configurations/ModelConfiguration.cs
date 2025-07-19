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
                    DateCreated = dateCreated
                },
                new Model
                {
                    Id = new("9910ba5f-faca-4790-88a4-352e71e14724"),
                    AIServiceId = AIServiceType.Ollama,
                    Name = "mistral",
                    DateCreated = dateCreated
                },
                new Model
                {
                    Id = new("1fe5381b-0262-469a-b63e-f4d0c4807a98"),
                    AIServiceId = AIServiceType.Ollama,
                    Name = "gemma3",
                    DateCreated = dateCreated
                }
            );
        }
    }
}
