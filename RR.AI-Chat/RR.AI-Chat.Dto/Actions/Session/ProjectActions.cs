using FluentValidation;

namespace RR.AI_Chat.Dto.Actions.Session
{
    public class UpsertProjectActionDto
    {
        public Guid? Id { get; set; }

        public string Name { get; set; } = null!;

        public string Description { get; set; } = null!;

        public string Instructions { get; set; } = null!;
    }

    public class UpsertProjectActionDtoValidator : AbstractValidator<UpsertProjectActionDto>
    {
        public UpsertProjectActionDtoValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(256);
            RuleFor(x => x.Description).NotEmpty().MaximumLength(1024);
            RuleFor(x => x.Instructions).NotEmpty().MaximumLength(2048);
        }
    }
}
