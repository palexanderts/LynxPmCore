using LynxPmCore.Application.DTOs;
using LynxPmCore.Shared.Abstractions;

namespace LynxPmCore.Application.Features.Analytics.Queries.GetDashboardKpis;

public sealed record GetDashboardKpisQuery : IQuery<DashboardKpisDto>;
