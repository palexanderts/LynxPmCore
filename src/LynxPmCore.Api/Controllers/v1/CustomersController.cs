using Asp.Versioning;
using LynxPmCore.Application.Features.Customers.Queries.GetCustomerEquipments;
using LynxPmCore.Application.Features.Customers.Queries.GetCustomers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LynxPmCore.Api.Controllers.v1;

[ApiController]
[ApiVersion(1)]
[Route("api/v{version:apiVersion}/customers")]
[Authorize]
public sealed class CustomersController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        var result = await sender.Send(new GetCustomersQuery(page, pageSize, search), ct);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpGet("{code}/equipments")]
    public async Task<IActionResult> GetEquipments(string code, CancellationToken ct)
    {
        var result = await sender.Send(new GetCustomerEquipmentsQuery(code), ct);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }
}
