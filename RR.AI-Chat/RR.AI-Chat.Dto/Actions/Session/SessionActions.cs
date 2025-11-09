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

    public class UpdateSessionActionDto
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = null!;

        public Guid? ProjectId { get; set; }
    }

    public class UpdateSessionActionDtoValidator : AbstractValidator<UpdateSessionActionDto>
    {
        public UpdateSessionActionDtoValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.Name).NotEmpty().MaximumLength(256);
        }
    }
}
