namespace LynxPmCore.Persistence.Legacy;

// Lectura de solo consulta contra las tablas legacy (LYNX_PM_AVISO / LYNX_PM_AVISO_OPERATIONS),
// usadas hoy por los formularios web. Nunca escribe ahí — es la fuente para el job de
// sincronización legacy -> LYNXCORE_*.
public interface ILegacyAvisoReader
{
    Task<IReadOnlyList<LegacyAvisoRecord>> GetAllAvisosAsync(CancellationToken ct = default);
    Task<IReadOnlyList<LegacyOperationRecord>> GetOperationsByAvisoIdAsync(string avisoId, CancellationToken ct = default);
}
