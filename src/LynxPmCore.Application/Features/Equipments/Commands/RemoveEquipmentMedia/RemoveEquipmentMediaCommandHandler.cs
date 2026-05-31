using LynxPmCore.Application.Common.Interfaces;
using LynxPmCore.Domain.Errors;
using LynxPmCore.Domain.Repositories;
using LynxPmCore.Shared.Abstractions;
using LynxPmCore.Shared.Common;

namespace LynxPmCore.Application.Features.Equipments.Commands.RemoveEquipmentMedia;

internal sealed class RemoveEquipmentMediaCommandHandler(
    IEquipmentRepository equipmentRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<RemoveEquipmentMediaCommand>
{
    public async Task<Result> Handle(RemoveEquipmentMediaCommand request, CancellationToken ct)
    {
        var media = await equipmentRepository.GetMediaByIdAsync(request.MediaId, ct);
        if (media is null)
            return Result.Failure(DomainErrors.Equipment.MediaNotFound);

        media.SoftDelete();
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}
