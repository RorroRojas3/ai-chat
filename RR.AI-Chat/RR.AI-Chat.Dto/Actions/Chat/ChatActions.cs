using FluentValidation;

namespace RR.AI_Chat.Dto.Actions.Chat
{
    public class CreateChatActionDto
    {
        public Guid? ProjectId { get; set; }
    }

    public class CreateChatActionDtoValidator : AbstractValidator<CreateChatActionDto>
    {
        public CreateChatActionDtoValidator()
        {
        }
    }

    public class CreateChatStreamActionDto
    {
        public string Prompt { get; set; } = null!;

        public Guid ModelId { get; set; }

        public Guid ServiceId { get; set; }

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
            RuleForEach(x => x.McpServers)
                .ChildRules(mcp =>
                {
                    mcp.RuleFor(m => m.Name)
                        .NotEmpty().WithMessage("MCP Server name is required.");
                })
                .When(x => x.McpServers != null && x.McpServers.Count > 0);
        }
    }

    public class DeactivateChatBulkActionDto
    {
        public List<Guid> ChatIds { get; set; } = [];
    }

    public class DeactivateChatBulkActionDtoValidator : AbstractValidator<DeactivateChatBulkActionDto>
    {
        public DeactivateChatBulkActionDtoValidator()
        {
            RuleFor(x => x.ChatIds).NotEmpty();
            RuleForEach(x => x.ChatIds).NotEmpty();
        }
    }
}
