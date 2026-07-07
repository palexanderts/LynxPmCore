using LynxPmCore.Application.Common.Interfaces;
using LynxPmCore.Domain.Repositories;
using LynxPmCore.Shared.Abstractions;
using LynxPmCore.Shared.Common;

namespace LynxPmCore.Application.Features.Permits.Queries;

internal sealed class HasPermitQueryHandler(
    IPermissionRepository permissionRepository,
    ICurrentUserService currentUser) : IQueryHandler<HasPermitQuery, bool>
{
    public async Task<Result<bool>> Handle(HasPermitQuery request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(currentUser.UserCode))
            return Result.Success(false);

        var allowed = await permissionRepository.HasPermitAsync(currentUser.UserCode, request.PermitDescription, ct);
        return Result.Success(allowed);
    }
}
