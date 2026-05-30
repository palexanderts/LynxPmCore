using FluentValidation;

namespace LynxPmCore.Application.Features.Notices.Commands.CreateNotice;

internal sealed class CreateNoticeCommandValidator : AbstractValidator<CreateNoticeCommand>
{
    public CreateNoticeCommandValidator()
    {
        RuleFor(x => x.Number).NotEmpty().MaximumLength(50);
        RuleFor(x => x.EquipmentCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Priority).InclusiveBetween(1, 5);
    }
}
