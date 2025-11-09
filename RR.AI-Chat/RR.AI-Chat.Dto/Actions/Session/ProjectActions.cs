using FluentValidation;
using System.Text.Json.Serialization;

namespace RR.AI_Chat.Dto.Actions.Session
{
    public class UpsertProjectActionDto
    {
        [JsonPropertyName("id")]
        public Guid? Id { get; set; }
        
        [JsonPropertyName("name")]
        public string Name { get; set; } = null!;

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("instructions")]
        public string? Instructions { get; set; } 
    }

    public class UpsertProjectActionDtoValidator : AbstractValidator<UpsertProjectActionDto>
    {
        public UpsertProjectActionDtoValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(256);
            RuleFor(x => x.Description)
                .MaximumLength(1024)
                .When(x => !string.IsNullOrWhiteSpace(x.Description));
            RuleFor(x => x.Instructions)
                .MaximumLength(2048)
                .When(x => !string.IsNullOrWhiteSpace(x.Instructions));
        }
    }
}
