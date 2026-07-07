using LynxPmCore.Domain.Primitives;

namespace LynxPmCore.Domain.Aggregates.Notices;

public sealed class OperationPart : IntEntity
{
    private OperationPart() { }

    public int OperationId { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string? Text { get; private set; }

    public static OperationPart Create(int operationId, string code, string? text)
    {
        return new OperationPart
        {
            OperationId = operationId,
            Code = code,
            Text = text
        };
    }
}
