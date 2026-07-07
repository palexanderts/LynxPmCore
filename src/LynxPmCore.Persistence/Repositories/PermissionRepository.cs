using System.Data;
using LynxPmCore.Domain.Repositories;
using LynxPmCore.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Oracle.ManagedDataAccess.Client;

namespace LynxPmCore.Persistence.Repositories;

// LYNX_PM_USERS/LYNX_PM_ROLS_PERMITS/LYNX_PM_PERMITS son catalogos de solo lectura
// sin comportamiento de negocio -- no se modelan como agregados de dominio, se
// consultan con SQL crudo sobre la misma conexion del LynxPmDbContext.
internal sealed class PermissionRepository(LynxPmDbContext db) : IPermissionRepository
{
    public async Task<bool> HasPermitAsync(string userLogon, string permitDescription, CancellationToken ct = default)
    {
        await EnsureOpenAsync(ct);

        var command = CreateCommand("""
            SELECT rp.VALUE
            FROM LYNX_PM_USERS u
            JOIN LYNX_PM_ROLS_PERMITS rp ON rp.ROL_ID = u.ROL_ID
            JOIN LYNX_PM_PERMITS p ON p.ID = rp.PERMIT
            WHERE u.LOGON = :userCode AND p.DESCRIPTION = :permitDescription AND u.IS_ACTIVE = '1'
            """);

        command.Parameters.Add(new OracleParameter("userCode", userLogon));
        command.Parameters.Add(new OracleParameter("permitDescription", permitDescription));

        using var reader = await command.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
            return false;

        var value = reader.IsDBNull(0) ? null : reader.GetString(0);
        return string.Equals(value?.Trim(), "true", StringComparison.OrdinalIgnoreCase);
    }

    private OracleCommand CreateCommand(string sql)
    {
        var connection = (OracleConnection)db.Database.GetDbConnection();
        var command = connection.CreateCommand();
        command.CommandText = sql;
        return command;
    }

    private async Task EnsureOpenAsync(CancellationToken ct)
    {
        if (db.Database.GetDbConnection().State != ConnectionState.Open)
            await db.Database.OpenConnectionAsync(ct);
    }
}
