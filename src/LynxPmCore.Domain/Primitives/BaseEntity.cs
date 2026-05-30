namespace LynxPmCore.Domain.Primitives;

public abstract class BaseEntity
{
    private readonly List<IDomainEvent> _domainEvents = [];

    protected BaseEntity() { }
    protected BaseEntity(Guid id) => Id = id;

    public Guid Id { get; protected set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; protected set; } = DateTime.UtcNow;
    public bool IsDeleted { get; protected set; }

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void RaiseDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
    public void ClearDomainEvents() => _domainEvents.Clear();
    protected void MarkUpdated() => UpdatedAt = DateTime.UtcNow;
    protected void MarkDeleted() { IsDeleted = true; MarkUpdated(); }
}
