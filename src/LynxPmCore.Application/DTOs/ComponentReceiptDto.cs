namespace LynxPmCore.Application.DTOs;

public sealed class ComponentReceiptDto
{
    public string ReceiptId { get; set; } = string.Empty;
    public string ComponentId { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string? Observations { get; set; }
    public string ReceivedBy { get; set; } = string.Empty;
    public DateTime ReceivedAt { get; set; }
}

public sealed class ComponentNotificationDto
{
    public int Id { get; set; }
    public string? AvisoId { get; set; }
    public int? OperationPosition { get; set; }
    public string? ProductCode { get; set; }
    public int? Quantity { get; set; }
    public string? UserCode { get; set; }
    public DateTime? Fecha { get; set; }
    public string? Comments { get; set; }
}

public sealed class ComponentCatalogItemDto
{
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? UnitOfMeasure { get; set; }
    public int StockQuantity { get; set; }
}
