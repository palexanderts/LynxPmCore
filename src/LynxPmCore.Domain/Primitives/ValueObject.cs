namespace LynxPmCore.Domain.Primitives;

public abstract class ValueObject : IEquatable<ValueObject>
{
    protected abstract IEnumerable<object?> GetEqualityComponents();

    public bool Equals(ValueObject? other) =>
        other is not null && GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());

    public override bool Equals(object? obj) => obj is ValueObject vo && Equals(vo);

    public override int GetHashCode() =>
        GetEqualityComponents().Aggregate(0, (hash, c) => HashCode.Combine(hash, c));

    public static bool operator ==(ValueObject? left, ValueObject? right) =>
        left?.Equals(right) ?? right is null;
    public static bool operator !=(ValueObject? left, ValueObject? right) => !(left == right);
}
