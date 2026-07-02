namespace LynxPmCore.Domain.Aggregates.Components;

// Mapea la tabla legacy LYNX_PM_COMPONENT_CENTER_STORE (stock real, por centro/almacén/ubicación).
// No hereda BaseEntity: esa tabla no tiene columnas de auditoría (CreatedAt/UpdatedAt/IsDeleted) ni PK.
public sealed class ComponentStockLocation
{
    private ComponentStockLocation() { }

    public int Id { get; private set; }
    public string ComponentCode { get; private set; } = string.Empty;
    public string? Center { get; private set; }
    public string? Store { get; private set; }
    public string? Location { get; private set; }
    public int Quantity { get; private set; }
}
