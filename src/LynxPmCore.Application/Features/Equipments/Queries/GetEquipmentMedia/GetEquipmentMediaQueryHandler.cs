using AutoMapper;
using LynxPmCore.Application.DTOs;
using LynxPmCore.Domain.Repositories;
using LynxPmCore.Shared.Abstractions;
using LynxPmCore.Shared.Common;

namespace LynxPmCore.Application.Features.Equipments.Queries.GetEquipmentMedia;

internal sealed class GetEquipmentMediaQueryHandler(
    IEquipmentRepository equipmentRepository,
    IMapper mapper) : IQueryHandler<GetEquipmentMediaQuery, IReadOnlyList<EquipmentMediaDto>>
{
    public async Task<Result<IReadOnlyList<EquipmentMediaDto>>> Handle(GetEquipmentMediaQuery request, CancellationToken ct)
    {
        var media = await equipmentRepository.GetMediaByCodeAsync(request.EquipmentCode, ct);
        return Result.Success<IReadOnlyList<EquipmentMediaDto>>(mapper.Map<List<EquipmentMediaDto>>(media));
    }
}
