namespace LynxPmCore.Application.DTOs;

public sealed class EquipmentDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string? Customer { get; set; }
    public string? CenterCode { get; set; }
    public string? ParentCode { get; set; }
    public bool IsActive { get; set; }
    public DateTime? LastSyncAt { get; set; }
    public List<EquipmentMediaDto> Media { get; set; } = [];
}

public sealed class EquipmentHistoryDto
{
    public string NoticeNumber { get; set; } = string.Empty;
    public string EquipmentCode { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string? Technician { get; set; }
}
