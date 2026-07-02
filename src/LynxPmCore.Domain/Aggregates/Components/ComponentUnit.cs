namespace LynxPmCore.Domain.Aggregates.Components;

// Mapea la tabla legacy LYNX_PM_COMPONENTS_UNITS (unidad de medida por componente).
public sealed class ComponentUnit
{
    private ComponentUnit() { }

    public int Id { get; private set; }
    public string ComponentCode { get; private set; } = string.Empty;
    public string? Unit { get; private set; }
}
