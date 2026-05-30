using LynxPmCore.Domain.Primitives;
using LynxPmCore.Shared.Common;

namespace LynxPmCore.Domain.ValueObjects;

public sealed class EquipmentCode : ValueObject
{
    public string Value { get; }

    private EquipmentCode(string value) => Value = value;

    public static Result<EquipmentCode> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<EquipmentCode>(new Error("EquipmentCode.Empty", "Equipment code cannot be empty."));
        return Result.Success<EquipmentCode>(new EquipmentCode(value.Trim().ToUpperInvariant()));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
