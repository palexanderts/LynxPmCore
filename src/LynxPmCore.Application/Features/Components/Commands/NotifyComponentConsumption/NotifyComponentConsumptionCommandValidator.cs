using FluentValidation;

namespace LynxPmCore.Application.Features.Components.Commands.NotifyComponentConsumption;

internal sealed class NotifyComponentConsumptionCommandValidator : AbstractValidator<NotifyComponentConsumptionCommand>
{
    public NotifyComponentConsumptionCommandValidator()
    {
        RuleFor(x => x.ComponentId).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.Observations).MaximumLength(200);
        RuleFor(x => x.ConsumedBy).NotEmpty().MaximumLength(10);
        RuleFor(x => x.AvisoId).MaximumLength(10);
    }
}
