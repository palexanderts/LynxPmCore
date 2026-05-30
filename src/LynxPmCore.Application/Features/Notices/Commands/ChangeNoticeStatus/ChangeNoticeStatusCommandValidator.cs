using FluentValidation;

namespace LynxPmCore.Application.Features.Notices.Commands.ChangeNoticeStatus;

internal sealed class ChangeNoticeStatusCommandValidator : AbstractValidator<ChangeNoticeStatusCommand>
{
    public ChangeNoticeStatusCommandValidator()
    {
        RuleFor(x => x.NoticeId).NotEmpty();
        RuleFor(x => x.NewStatus).IsInEnum();
    }
}
