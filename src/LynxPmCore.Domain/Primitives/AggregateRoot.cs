namespace LynxPmCore.Domain.Primitives;

public abstract class AggregateRoot : BaseEntity
{
    protected AggregateRoot() { }
    protected AggregateRoot(Guid id) : base(id) { }
}
