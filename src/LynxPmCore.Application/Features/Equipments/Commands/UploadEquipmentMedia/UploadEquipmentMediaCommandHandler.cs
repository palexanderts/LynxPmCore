using AutoMapper;
using LynxPmCore.Application.Common.Interfaces;
using LynxPmCore.Application.DTOs;
using LynxPmCore.Domain.Aggregates.Equipments;
using LynxPmCore.Domain.Errors;
using LynxPmCore.Domain.Repositories;
using LynxPmCore.Shared.Abstractions;
using LynxPmCore.Shared.Common;

namespace LynxPmCore.Application.Features.Equipments.Commands.UploadEquipmentMedia;

internal sealed class UploadEquipmentMediaCommandHandler(
    IEquipmentRepository equipmentRepository,
    IFileStorageService fileStorage,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUser,
    IMapper mapper) : ICommandHandler<UploadEquipmentMediaCommand, EquipmentMediaDto>
{
    public async Task<Result<EquipmentMediaDto>> Handle(UploadEquipmentMediaCommand request, CancellationToken ct)
    {
        var equipment = await equipmentRepository.GetByCodeAsync(request.EquipmentCode, ct);
        if (equipment is null)
            return Result.Failure<EquipmentMediaDto>(DomainErrors.Equipment.NotFound);

        var url = await fileStorage.StoreAsync(
            request.FileStream,
            request.FileName,
            request.ContentType,
            request.EquipmentCode,
            ct);

        var media = EquipmentMedia.Create(
            request.EquipmentCode,
            request.MediaType.ToUpperInvariant(),
            url,
            thumbnailUrl: null,
            request.Title,
            request.Position,
            currentUser.UserCode);

        await equipmentRepository.AddMediaAsync(media, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success(mapper.Map<EquipmentMediaDto>(media));
    }
}
