using MediatR;

namespace LynxPmCore.Domain.Primitives;

public interface IDomainEvent : INotification
{
    Guid Id { get; }
    DateTime OccurredOnUtc { get; }
}
