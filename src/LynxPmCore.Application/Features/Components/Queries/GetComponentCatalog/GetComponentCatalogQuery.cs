using LynxPmCore.Application.DTOs;
using LynxPmCore.Shared.Abstractions;
using LynxPmCore.Shared.Common;

namespace LynxPmCore.Application.Features.Components.Queries.GetComponentCatalog;

public sealed record GetComponentCatalogQuery(
    int Page = 1,
    int PageSize = 50,
    string? Search = null) : IQuery<PagedResult<ComponentCatalogItemDto>>;
