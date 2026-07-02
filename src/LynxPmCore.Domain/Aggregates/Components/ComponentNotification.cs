namespace LynxPmCore.Domain.Aggregates.Components;

// Mapea la tabla legacy LYNX_PM_COMPONENT_NOTIFICATIONS — destino real de "notificar consumo
// de componente", vinculado al aviso y a la posición de la operación. La tabla no tiene
// trigger/identity para ID; el repositorio obtiene el valor de NOTIFY_OPERATION_SEQ antes de crear.
public sealed class ComponentNotification
{
    private ComponentNotification() { }

    public int Id { get; private set; }
    public string? AvisoId { get; private set; }
    public int? OperationPosition { get; private set; }
    public string? ProductCode { get; private set; }
    public int? Quantity { get; private set; }
    public string? UserCode { get; private set; }
    public DateTime? Fecha { get; private set; }
    public string? Comments { get; private set; }

    public static ComponentNotification Create(
        int id,
        string? avisoId,
        int? operationPosition,
        string productCode,
        int quantity,
        string userCode,
        string? comments)
    {
        return new ComponentNotification
        {
            Id = id,
            AvisoId = avisoId,
            OperationPosition = operationPosition,
            ProductCode = productCode,
            Quantity = quantity,
            UserCode = userCode,
            Fecha = DateTime.UtcNow,
            Comments = comments
        };
    }
}
