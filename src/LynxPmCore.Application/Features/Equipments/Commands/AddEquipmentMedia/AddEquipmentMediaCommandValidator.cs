using FluentValidation;

namespace LynxPmCore.Application.Features.Equipments.Commands.AddEquipmentMedia;

internal sealed class AddEquipmentMediaCommandValidator : AbstractValidator<AddEquipmentMediaCommand>
{
    public AddEquipmentMediaCommandValidator()
    {
        RuleFor(x => x.EquipmentCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.MediaType).NotEmpty().Must(t => t.ToUpperInvariant() is "IMAGE" or "VIDEO")
            .WithMessage("MediaType must be IMAGE or VIDEO.");
        RuleFor(x => x.Url).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.ThumbnailUrl).MaximumLength(2000).When(x => x.ThumbnailUrl is not null);
        RuleFor(x => x.Title).MaximumLength(200).When(x => x.Title is not null);
        RuleFor(x => x.Position).GreaterThanOrEqualTo(0);
    }
}
