using AutoMapper;
using LynxPmCore.Application.DTOs;
using LynxPmCore.Domain.Repositories;
using LynxPmCore.Shared.Abstractions;
using LynxPmCore.Shared.Common;

namespace LynxPmCore.Application.Features.Customers.Queries.GetCustomers;

internal sealed class GetCustomersQueryHandler(
    ICustomerRepository customerRepository,
    IMapper mapper) : IQueryHandler<GetCustomersQuery, PagedResult<CustomerDto>>
{
    public async Task<Result<PagedResult<CustomerDto>>> Handle(GetCustomersQuery request, CancellationToken ct)
    {
        var (items, total) = await customerRepository.GetPagedAsync(
            request.Page, request.PageSize, request.Search, ct);

        var dtos = mapper.Map<IReadOnlyList<CustomerDto>>(items);
        return Result.Success(new PagedResult<CustomerDto>(dtos, request.Page, request.PageSize, total));
    }
}
