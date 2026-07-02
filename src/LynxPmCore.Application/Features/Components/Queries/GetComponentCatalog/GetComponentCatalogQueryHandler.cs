using LynxPmCore.Application.DTOs;
using LynxPmCore.Domain.Repositories;
using LynxPmCore.Shared.Abstractions;
using LynxPmCore.Shared.Common;

namespace LynxPmCore.Application.Features.Components.Queries.GetComponentCatalog;

// Combina 3 tablas legacy (LYNX_PM_COMPONENT_CENTER_STORE, LYNX_PM_COMPONENTS_UNITS,
// LYNX_PM_COMPONENTS) en un catálogo único. El dataset es pequeño (decenas de filas), por lo
// que se combina en memoria en vez de intentar un join EF complejo sobre tablas sin relación
// declarada (no hay FKs entre ellas).
internal sealed class GetComponentCatalogQueryHandler(
    IComponentCatalogRepository repo) : IQueryHandler<GetComponentCatalogQuery, PagedResult<ComponentCatalogItemDto>>
{
    public async Task<Result<PagedResult<ComponentCatalogItemDto>>> Handle(GetComponentCatalogQuery request, CancellationToken ct)
    {
        var stock = await repo.GetAllStockAsync(ct);
        var units = await repo.GetAllUnitsAsync(ct);
        var masters = await repo.GetAllMastersAsync(ct);

        var unitByCode = units
            .Where(u => !string.IsNullOrWhiteSpace(u.ComponentCode))
            .GroupBy(u => u.ComponentCode!, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First().Unit, StringComparer.OrdinalIgnoreCase);

        var descriptionByCode = masters
            .Where(m => !string.IsNullOrWhiteSpace(m.Code) && !string.IsNullOrWhiteSpace(m.Name))
            .GroupBy(m => m.Code!, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First().Name!, StringComparer.OrdinalIgnoreCase);

        var items = stock
            .Where(s => !string.IsNullOrWhiteSpace(s.ComponentCode))
            .GroupBy(s => s.ComponentCode, StringComparer.OrdinalIgnoreCase)
            .Select(g => new ComponentCatalogItemDto
            {
                Code = g.Key,
                Description = descriptionByCode.GetValueOrDefault(g.Key, g.Key),
                UnitOfMeasure = unitByCode.GetValueOrDefault(g.Key),
                StockQuantity = g.Sum(s => s.Quantity)
            })
            .AsEnumerable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search;
            items = items.Where(i =>
                i.Code.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                i.Description.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        var all = items.OrderBy(i => i.Code).ToList();
        var total = all.Count;
        var paged = all.Skip((request.Page - 1) * request.PageSize).Take(request.PageSize).ToList();

        return Result.Success(new PagedResult<ComponentCatalogItemDto>(paged, request.Page, request.PageSize, total));
    }
}
