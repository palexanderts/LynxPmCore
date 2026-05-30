using LynxPmCore.Application.DTOs;
using LynxPmCore.Shared.Abstractions;
using LynxPmCore.Shared.Common;

namespace LynxPmCore.Application.Features.Customers.Queries.GetCustomers;

public sealed record GetCustomersQuery(
    int Page = 1,
    int PageSize = 50,
    string? Search = null) : IQuery<PagedResult<CustomerDto>>;
