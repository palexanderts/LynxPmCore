using LynxPmCore.Domain.Enums;

namespace LynxPmCore.Application.DTOs;

public sealed class NoticeDto
{
    public Guid Id { get; set; }
    public string Number { get; set; } = string.Empty;
    public string EquipmentCode { get; set; } = string.Empty;
    public string? Description { get; set; }
    public NoticeStatus Status { get; set; }
    public string StatusName => Status.ToString();
    public bool IsApproved { get; set; }
    public NoticeApprovalStatus ApprovalStatus { get; set; }
    public string ApprovalStatusName => ApprovalStatus.ToString();
    public string? ApprovedBy { get; set; }
    public string? RejectionReason { get; set; }
    public bool IsSynchronized { get; set; }
    public string? ApexId { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string? Customer { get; set; }
    public string? Center { get; set; }
    public int Priority { get; set; }
    public string? PriorityCode { get; set; }
    public string? PriorityText { get; set; }
    public string? NoticeTypeCode { get; set; }
    public string? NoticeTypeText { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<OperationDto> Operations { get; set; } = [];
    public List<EquipmentMediaDto> EquipmentMedia { get; set; } = [];
}

public sealed class NoticeListDto
{
    public Guid Id { get; set; }
    public string Number { get; set; } = string.Empty;
    public string EquipmentCode { get; set; } = string.Empty;
    public string? Description { get; set; }
    public NoticeStatus Status { get; set; }
    public string StatusName => Status.ToString();
    public bool IsApproved { get; set; }
    public NoticeApprovalStatus ApprovalStatus { get; set; }
    public string ApprovalStatusName => ApprovalStatus.ToString();
    public bool IsSynchronized { get; set; }
    public string? Location { get; set; }
    public string? Customer { get; set; }
    public int Priority { get; set; }
    public DateTime CreatedAt { get; set; }
}

public sealed class NoticeChangeDto
{
    public string Number { get; set; } = string.Empty;
    public string EquipmentCode { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsApproved { get; set; }
    public string? ApexId { get; set; }
    public DateTime UpdatedAt { get; set; }
}
