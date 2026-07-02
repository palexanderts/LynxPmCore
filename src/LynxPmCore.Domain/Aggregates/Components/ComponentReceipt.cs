using LynxPmCore.Domain.Primitives;

namespace LynxPmCore.Domain.Aggregates.Components;

public sealed class ComponentReceipt : BaseEntity
{
    private ComponentReceipt() { }

    public string ReceiptId { get; private set; } = string.Empty;
    public string ComponentId { get; private set; } = string.Empty;
    public int Quantity { get; private set; }
    public string? Observations { get; private set; }
    public string ReceivedBy { get; private set; } = string.Empty;
    public DateTime ReceivedAt { get; private set; }

    public static ComponentReceipt Create(
        string componentId,
        int quantity,
        string? observations,
        string receivedBy,
        DateTime receivedAt)
    {
        return new ComponentReceipt
        {
            ReceiptId = "REC-" + DateTime.UtcNow.ToString("yyyyMMdd") + "-" + Guid.NewGuid().ToString("N")[..6].ToUpper(),
            ComponentId = componentId,
            Quantity = quantity,
            Observations = observations,
            ReceivedBy = receivedBy,
            ReceivedAt = receivedAt
        };
    }
}
