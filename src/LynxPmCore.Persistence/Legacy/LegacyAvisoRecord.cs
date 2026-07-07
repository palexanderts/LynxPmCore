namespace LynxPmCore.Persistence.Legacy;

// Fila cruda de LYNX_PM_AVISO (sistema legacy, usado por los formularios web).
// Solo lectura — no forma parte del modelo de dominio.
public sealed class LegacyAvisoRecord
{
    public int Id { get; set; }
    public string? SapAvisoId { get; set; }
    public string? AvisoSap { get; set; }
    public string? Equipment { get; set; }
    public string? CenterPlan { get; set; }
    public string? Customer { get; set; }
    public string? Usuario { get; set; }
    public int Status { get; set; }
}
