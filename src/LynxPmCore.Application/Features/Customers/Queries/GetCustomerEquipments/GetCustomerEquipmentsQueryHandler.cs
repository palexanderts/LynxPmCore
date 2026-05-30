using AutoMapper;
using LynxPmCore.Application.DTOs;
using LynxPmCore.Domain.Repositories;
using LynxPmCore.Shared.Abstractions;
using LynxPmCore.Shared.Common;

namespace LynxPmCore.Application.Features.Customers.Queries.GetCustomerEquipments;

internal sealed class GetCustomerEquipmentsQueryHandler(
    IEquipmentRepository equipmentRepository,
    IMapper mapper) : IQueryHandler<GetCustomerEquipmentsQuery, IReadOnlyList<EquipmentDto>>
{
    public async Task<Result<IReadOnlyList<EquipmentDto>>> Handle(GetCustomerEquipmentsQuery request, CancellationToken ct)
    {
        var items = await equipmentRepository.GetByCustomerAsync(request.CustomerCode, ct);
        return Result.Success(mapper.Map<IReadOnlyList<EquipmentDto>>(items));
    }
}
