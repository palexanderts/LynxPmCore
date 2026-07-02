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
        RuleFor(x => x.PriorityCode).MaximumLength(10);
        RuleFor(x => x.PriorityText).MaximumLength(100);
        RuleFor(x => x.NoticeTypeCode).MaximumLength(10);
        RuleFor(x => x.NoticeTypeText).MaximumLength(100);
        RuleFor(x => x.Center).MaximumLength(50);
    }
}
