using LynxPmCore.Shared.Abstractions;

namespace LynxPmCore.Application.Features.Permits.Queries;

public sealed record HasPermitQuery(string PermitDescription) : IQuery<bool>;
