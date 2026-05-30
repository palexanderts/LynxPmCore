using AutoMapper;
using LynxPmCore.Application.DTOs;
using LynxPmCore.Domain.Repositories;
using LynxPmCore.Shared.Abstractions;
using LynxPmCore.Shared.Common;

namespace LynxPmCore.Application.Features.Equipments.Queries.GetEquipments;

internal sealed class GetEquipmentsQueryHandler(
    IEquipmentRepository equipmentRepository,
    IMapper mapper) : IQueryHandler<GetEquipmentsQuery, PagedResult<EquipmentDto>>
{
    public async Task<Result<PagedResult<EquipmentDto>>> Handle(GetEquipmentsQuery request, CancellationToken ct)
    {
        var (items, total) = await equipmentRepository.GetPagedAsync(
            request.Page, request.PageSize, request.Search, request.Customer, ct);

        var dtos = mapper.Map<IReadOnlyList<EquipmentDto>>(items);
        return Result.Success(new PagedResult<EquipmentDto>(dtos, request.Page, request.PageSize, total));
    }
}
