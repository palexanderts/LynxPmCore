using LynxPmCore.Domain.Primitives;

namespace LynxPmCore.Domain.Aggregates.Notices;

public sealed class OperationCause : BaseEntity
{
    private OperationCause() { }

    public Guid OperationId { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string? Text { get; private set; }

    public static OperationCause Create(Guid operationId, string code, string? text)
    {
        return new OperationCause
        {
            OperationId = operationId,
            Code = code,
            Text = text
        };
    }
}
