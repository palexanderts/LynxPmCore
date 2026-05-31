using AutoMapper;
using LynxPmCore.Application.Common.Interfaces;
using LynxPmCore.Application.DTOs;
using LynxPmCore.Domain.Aggregates.Equipments;
using LynxPmCore.Domain.Errors;
using LynxPmCore.Domain.Repositories;
using LynxPmCore.Shared.Abstractions;
using LynxPmCore.Shared.Common;

namespace LynxPmCore.Application.Features.Equipments.Commands.AddEquipmentMedia;

internal sealed class AddEquipmentMediaCommandHandler(
    IEquipmentRepository equipmentRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUser,
    IMapper mapper) : ICommandHandler<AddEquipmentMediaCommand, EquipmentMediaDto>
{
    public async Task<Result<EquipmentMediaDto>> Handle(AddEquipmentMediaCommand request, CancellationToken ct)
    {
        var equipment = await equipmentRepository.GetByCodeAsync(request.EquipmentCode, ct);
        if (equipment is null)
            return Result.Failure<EquipmentMediaDto>(DomainErrors.Equipment.NotFound);

        var media = EquipmentMedia.Create(
            request.EquipmentCode,
            request.MediaType,
            request.Url,
            request.ThumbnailUrl,
            request.Title,
            request.Position,
            currentUser.UserCode);

        await equipmentRepository.AddMediaAsync(media, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success(mapper.Map<EquipmentMediaDto>(media));
    }
}
