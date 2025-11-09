using FluentValidation;

namespace RR.AI_Chat.Dto.Actions.Session
{
    public class CreateSessionActionDto
    {
        public Guid? ProjectId { get; set; }
    }

    public class CreateSessionActionDtoValidator : AbstractValidator<CreateSessionActionDto>
    {
        public CreateSessionActionDtoValidator()
        {
            RuleFor(x => x.ProjectId).NotEmpty().When(x => x.ProjectId.HasValue);
        }
    }

    public class DeactivateSessionBulkActionDto
    {
        public List<Guid> SessionIds { get; set; } = [];
    }

    public class DeactivateSessionBulkActionDtoValidator : AbstractValidator<DeactivateSessionBulkActionDto>
    {
        public DeactivateSessionBulkActionDtoValidator()
        {
            RuleFor(x => x.SessionIds).NotEmpty();
            RuleForEach(x => x.SessionIds).NotEmpty();
        }
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
}
