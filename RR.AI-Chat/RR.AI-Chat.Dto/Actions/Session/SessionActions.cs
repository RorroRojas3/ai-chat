using FluentValidation;

namespace RR.AI_Chat.Dto.Actions.Session
{
    public class DeactivateSessionBulkActionDto
    {
        public List<Guid> SessionIds { get; set; } = [];
    }

    public class RenameSessionActionDto
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = null!;
    }

    public class RenameSessionActionDtoValidator : AbstractValidator<RenameSessionActionDto>
    {
        public RenameSessionActionDtoValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.Name).NotEmpty().MaximumLength(256);
        }
    }

    public class CreateSessionProjectActionDto
    {
        public Guid SessionId { get; set; }

        public string Name { get; set; } = null!;   

        public string Instructions { get; set; } = null!;   
    }

    public class CreateSessionProjectActionDtoValidator : AbstractValidator<CreateSessionProjectActionDto>
    {
        public CreateSessionProjectActionDtoValidator()
        {
            RuleFor(x => x.SessionId).NotEmpty();
            RuleFor(x => x.Name).NotEmpty().MaximumLength(256);
            RuleFor(x => x.Instructions).NotEmpty().MaximumLength(2048);
        }
    }
}
