using System.Data;
using LynxPmCore.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Oracle.ManagedDataAccess.Client;

namespace LynxPmCore.Persistence.Legacy;

internal sealed class LegacyAvisoReader(LynxPmDbContext db) : ILegacyAvisoReader
{
    public async Task<IReadOnlyList<LegacyAvisoRecord>> GetAllAvisosAsync(CancellationToken ct = default)
    {
        var results = new List<LegacyAvisoRecord>();

        await EnsureOpenAsync(ct);
        await using var command = CreateCommand(
            "SELECT ID, SAPAVISOID, AVISOSAP, EQUIPMENT, CENTERPLAN, CUSTOMER, USUARIO, STATUS FROM LYNX_PM_AVISO");

        await using var reader = await command.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            results.Add(new LegacyAvisoRecord
            {
                Id = reader.GetInt32(0),
                SapAvisoId = reader.IsDBNull(1) ? null : reader.GetString(1),
                AvisoSap = reader.IsDBNull(2) ? null : reader.GetString(2),
                Equipment = reader.IsDBNull(3) ? null : reader.GetString(3),
                CenterPlan = reader.IsDBNull(4) ? null : reader.GetString(4),
                Customer = reader.IsDBNull(5) ? null : reader.GetString(5),
                Usuario = reader.IsDBNull(6) ? null : reader.GetString(6),
                Status = reader.IsDBNull(7) ? 0 : reader.GetInt32(7)
            });
        }

        return results;
    }

    public async Task<IReadOnlyList<LegacyOperationRecord>> GetOperationsByAvisoIdAsync(string avisoId, CancellationToken ct = default)
    {
        var results = new List<LegacyOperationRecord>();

        await EnsureOpenAsync(ct);
        await using var command = CreateCommand(
            "SELECT ID, OPERATIONCODE, OPERACIONCODE, DESCRIPTION, POSITION, STATUS, " +
            "STARTEXECUTIONDATE, ENDEXECUTIONDATE, EXECUTORCODE " +
            "FROM LYNX_PM_AVISO_OPERATIONS WHERE AVISOID = :avisoId");
        command.Parameters.Add(new OracleParameter("avisoId", avisoId));

        await using var reader = await command.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            results.Add(new LegacyOperationRecord
            {
                Id = reader.GetInt32(0),
                OperationCode = reader.IsDBNull(1) ? null : reader.GetString(1),
                OperacionCode = reader.IsDBNull(2) ? null : reader.GetString(2),
                Description = reader.IsDBNull(3) ? null : reader.GetString(3),
                Position = reader.IsDBNull(4) ? null : reader.GetInt32(4),
                Status = reader.IsDBNull(5) ? 0 : reader.GetInt32(5),
                StartExecutionDate = reader.IsDBNull(6) ? null : reader.GetDateTime(6),
                EndExecutionDate = reader.IsDBNull(7) ? null : reader.GetDateTime(7),
                ExecutorCode = reader.IsDBNull(8) ? null : reader.GetString(8)
            });
        }

        return results;
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
        {
            await db.Database.OpenConnectionAsync(ct);
        }
    }
}
