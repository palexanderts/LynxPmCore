using FluentValidation;

namespace LynxPmCore.Application.Features.Components.Commands.ReceiveComponent;

internal sealed class ReceiveComponentCommandValidator : AbstractValidator<ReceiveComponentCommand>
{
    public ReceiveComponentCommandValidator()
    {
        RuleFor(x => x.ComponentId).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.Observations).MaximumLength(1000);
        RuleFor(x => x.ReceivedBy).NotEmpty().MaximumLength(100);
        RuleFor(x => x.ReceivedAt)
            .NotEmpty()
            .LessThanOrEqualTo(_ => DateTime.UtcNow.AddMinutes(5))
            .WithMessage("ReceivedAt must be a valid date not in the future.");
    }
}
