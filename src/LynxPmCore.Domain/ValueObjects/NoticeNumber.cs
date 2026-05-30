using LynxPmCore.Domain.Primitives;
using LynxPmCore.Shared.Common;

namespace LynxPmCore.Domain.ValueObjects;

public sealed class NoticeNumber : ValueObject
{
    public string Value { get; }

    private NoticeNumber(string value) => Value = value;

    public static Result<NoticeNumber> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<NoticeNumber>(new Error("NoticeNumber.Empty", "Notice number cannot be empty."));
        return Result.Success<NoticeNumber>(new NoticeNumber(value.Trim().ToUpperInvariant()));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
