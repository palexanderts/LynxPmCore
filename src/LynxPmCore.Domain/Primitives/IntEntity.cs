namespace LynxPmCore.Domain.Primitives;

// Contraparte de BaseEntity para agregados cuyo Id lo genera Oracle (trigger + sequence)
// en vez de asignarse en el cliente, como Notice/Operation al apuntar directo a las
// tablas legacy LYNX_PM_AVISO*. No se toca BaseEntity porque el resto del dominio
// (Customer, Equipment, ComponentReceipt, etc.) sigue usando Guid.
public abstract class IntEntity : IHasDomainEvents
{
    private readonly List<IDomainEvent> _domainEvents = [];

    public int Id { get; protected set; }
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; protected set; } = DateTime.UtcNow;
    public bool IsDeleted { get; protected set; }

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void RaiseDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
    public void ClearDomainEvents() => _domainEvents.Clear();
    protected void MarkUpdated() => UpdatedAt = DateTime.UtcNow;
    protected void MarkDeleted() { IsDeleted = true; MarkUpdated(); }
}
