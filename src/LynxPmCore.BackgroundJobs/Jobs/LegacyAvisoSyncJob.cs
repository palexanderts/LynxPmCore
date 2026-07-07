using LynxPmCore.Application.Common.Interfaces;
using LynxPmCore.Domain.Aggregates.Notices;
using LynxPmCore.Domain.Enums;
using LynxPmCore.Domain.Repositories;
using LynxPmCore.Persistence.Legacy;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LynxPmCore.BackgroundJobs.Jobs;

// Importa avisos/operaciones desde el sistema legacy (LYNX_PM_AVISO / LYNX_PM_AVISO_OPERATIONS,
// usado por los formularios web) hacia LYNXCORE_AVISO / LYNXCORE_OPERATIONS (usado por la API
// REST + app móvil). Un solo sentido: legacy -> nuevo. Los formularios web no se tocan.
// Idempotente: usa Notice.ApexId / Operation.LegacySourceId para no duplicar en cada corrida.
public sealed class LegacyAvisoSyncJob(
    IServiceScopeFactory scopeFactory,
    ILogger<LegacyAvisoSyncJob> logger)
{
    public async Task ExecuteAsync()
    {
        using var scope = scopeFactory.CreateScope();
        var legacyReader = scope.ServiceProvider.GetRequiredService<ILegacyAvisoReader>();
        var noticeRepository = scope.ServiceProvider.GetRequiredService<INoticeRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var legacyAvisos = await legacyReader.GetAllAvisosAsync();
        logger.LogInformation("Legacy aviso sync: {Count} avisos legacy encontrados", legacyAvisos.Count);

        var importedNotices = 0;
        var importedOperations = 0;

        foreach (var legacyAviso in legacyAvisos)
        {
            var apexId = legacyAviso.Id.ToString();
            var notice = await noticeRepository.GetByApexIdAsync(apexId);

            if (notice is null)
            {
                var number = !string.IsNullOrWhiteSpace(legacyAviso.SapAvisoId)
                    ? legacyAviso.SapAvisoId!
                    : !string.IsNullOrWhiteSpace(legacyAviso.AvisoSap)
                        ? legacyAviso.AvisoSap!
                        : $"LEGACY-{legacyAviso.Id}";

                var createResult = Notice.Create(
                    number: number,
                    equipmentCode: legacyAviso.Equipment ?? "SIN-EQUIPO",
                    description: $"Importado del sistema legacy (aviso #{legacyAviso.Id})",
                    createdBy: string.IsNullOrWhiteSpace(legacyAviso.Usuario) ? "legacy-sync" : legacyAviso.Usuario!,
                    customer: legacyAviso.Customer,
                    center: legacyAviso.CenterPlan);

                if (createResult.IsFailure)
                {
                    logger.LogWarning(
                        "No se pudo importar aviso legacy {LegacyId}: {Error}",
                        legacyAviso.Id, createResult.Error.Description);
                    continue;
                }

                notice = createResult.Value;
                notice.MarkSynchronized(apexId);
                await noticeRepository.AddAsync(notice);
                await unitOfWork.SaveChangesAsync();
                importedNotices++;
            }

            var legacyOperations = await legacyReader.GetOperationsByAvisoIdAsync(apexId);
            var existingLegacySourceIds = notice.Operations
                .Where(o => o.LegacySourceId is not null)
                .Select(o => o.LegacySourceId)
                .ToHashSet();

            var addedOperationToNotice = false;

            foreach (var legacyOperation in legacyOperations)
            {
                var legacyOperationId = legacyOperation.Id.ToString();
                if (existingLegacySourceIds.Contains(legacyOperationId))
                {
                    continue;
                }

                var code = legacyOperation.OperationCode
                    ?? legacyOperation.OperacionCode
                    ?? $"OP-{legacyOperation.Id}";

                var operation = Operation.CreateFromLegacyImport(
                    noticeId: notice.Id,
                    code: code,
                    description: legacyOperation.Description ?? code,
                    status: MapLegacyOperationStatus(legacyOperation.Status),
                    position: legacyOperation.Position ?? 0,
                    startedAt: legacyOperation.StartExecutionDate,
                    completedAt: legacyOperation.EndExecutionDate,
                    assignedTechnician: legacyOperation.ExecutorCode,
                    legacySourceId: legacyOperationId);

                var addResult = notice.AddOperation(operation);
                if (addResult.IsFailure)
                {
                    logger.LogWarning(
                        "No se pudo importar operación legacy {LegacyOpId} del aviso {ApexId}: {Error}",
                        legacyOperation.Id, apexId, addResult.Error.Description);
                    continue;
                }

                importedOperations++;
                addedOperationToNotice = true;
            }

            if (addedOperationToNotice)
            {
                await noticeRepository.UpdateAsync(notice);
                await unitOfWork.SaveChangesAsync();
            }
        }

        logger.LogInformation(
            "Legacy aviso sync completado: {ImportedNotices} avisos nuevos, {ImportedOperations} operaciones nuevas",
            importedNotices, importedOperations);
    }

    private static OperationStatus MapLegacyOperationStatus(int legacyStatus) => legacyStatus switch
    {
        0 => OperationStatus.Pending,
        1 => OperationStatus.InProgress,
        2 => OperationStatus.Paused,
        _ => OperationStatus.Completed
    };
}
