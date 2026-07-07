using LynxPmCore.Domain.Enums;

namespace LynxPmCore.Application.DTOs;

public sealed class OperationDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public OperationType Type { get; set; }
    public string TypeName => Type.ToString();
    public OperationStatus Status { get; set; }
    public string StatusName => Status.ToString();
    public int Position { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? Notes { get; set; }
    public string? AssignedTechnician { get; set; }
    public string? Failure { get; set; }
    public List<OperationPartDto> Parts { get; set; } = [];
}

public sealed class OperationPartDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string? Text { get; set; }
}
