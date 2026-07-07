namespace LynxPmCore.Persistence.Legacy;

// Fila cruda de LYNX_PM_AVISO_OPERATIONS (sistema legacy). Solo lectura.
public sealed class LegacyOperationRecord
{
    public int Id { get; set; }
    public string? OperationCode { get; set; }
    public string? OperacionCode { get; set; }
    public string? Description { get; set; }
    public int? Position { get; set; }
    public int Status { get; set; }
    public DateTime? StartExecutionDate { get; set; }
    public DateTime? EndExecutionDate { get; set; }
    public string? ExecutorCode { get; set; }
}
