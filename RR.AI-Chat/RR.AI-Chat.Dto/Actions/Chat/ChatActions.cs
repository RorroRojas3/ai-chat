using FluentValidation;

namespace RR.AI_Chat.Dto.Actions.Chat
{
    public class CreateChatStreamActionDto
    {
        public string Prompt { get; set; } = null!;

        public Guid ModelId { get; set; }

        public Guid ServiceId { get; set; }

        public Guid? ProjectId { get; set; }

        public List<McpDto> McpServers { get; set; } = [];
    }

    public class CreateChatStreamActionDtoValidator : AbstractValidator<CreateChatStreamActionDto>
    {
        public CreateChatStreamActionDtoValidator()
        {
            RuleFor(x => x.Prompt)
                .NotEmpty().WithMessage("Prompt is required.");
            RuleFor(x => x.ModelId)
                .NotEmpty().WithMessage("ModelId is required.");
            RuleFor(x => x.ServiceId)
                .NotEmpty().WithMessage("ServiceId is required.");
        }
    }
}
