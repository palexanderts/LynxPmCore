using LynxPmCore.Application.DTOs;
using LynxPmCore.Shared.Abstractions;

namespace LynxPmCore.Application.Features.Customers.Queries.GetCustomerEquipments;

public sealed record GetCustomerEquipmentsQuery(string CustomerCode) : IQuery<IReadOnlyList<EquipmentDto>>;
