namespace LynxPmCore.Application.DTOs;

public sealed class EquipmentMediaDto
{
    public Guid Id { get; set; }
    public string EquipmentCode { get; set; } = string.Empty;
    public string MediaType { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public string? Title { get; set; }
    public int Position { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
}
