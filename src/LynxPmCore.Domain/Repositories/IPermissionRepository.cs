namespace LynxPmCore.Domain.Repositories;

public interface IPermissionRepository
{
    Task<bool> HasPermitAsync(string userLogon, string permitDescription, CancellationToken ct = default);
}
