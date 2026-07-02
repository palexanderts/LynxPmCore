namespace LynxPmCore.Domain.Aggregates.Components;

// Mapea la tabla legacy LYNX_PM_COMPONENTS (maestro/descripción). Alimentada hoy por datos
// de prueba no relacionados a los códigos reales de LYNX_PM_COMPONENT_CENTER_STORE; se usa
// como fuente de descripción "best effort" — si no hay coincidencia, se muestra el código.
public sealed class ComponentMaster
{
    private ComponentMaster() { }

    public int Id { get; private set; }
    public string? Code { get; private set; }
    public string? Name { get; private set; }
    public string? Type { get; private set; }
    public string? UnitBase { get; private set; }
    public string? ManufacturePart { get; private set; }
    public string? Location { get; private set; }
    public DateTime? LastChange { get; private set; }
    public string? IsDeleted { get; private set; }
}
