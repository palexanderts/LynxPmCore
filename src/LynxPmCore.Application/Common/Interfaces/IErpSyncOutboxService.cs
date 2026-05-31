using LynxPmCore.Domain.Enums;

namespace LynxPmCore.Application.Common.Interfaces;

/// <summary>
/// Encola un registro en el outbox ERP si el proceso está habilitado para el cliente.
/// Llamar desde los handlers de los 4 procesos que sincronizan con ERP.
/// </summary>
public interface IErpSyncOutboxService
{
    Task EnqueueAsync(ErpSyncProcess process, string entityId, object payload, CancellationToken ct = default);
}
