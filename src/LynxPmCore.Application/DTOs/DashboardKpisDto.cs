namespace LynxPmCore.Application.DTOs;

public sealed class DashboardKpisDto
{
    public NoticeKpisDto Notices { get; set; } = new();
    public EquipmentKpisDto Equipment { get; set; } = new();
    public SyncKpisDto Sync { get; set; } = new();
}

public sealed class NoticeKpisDto
{
    public int Total { get; set; }
    public int PendingApproval { get; set; }
    public double AvgResolutionHours { get; set; }
    public Dictionary<string, int> ByStatus { get; set; } = [];
    public Dictionary<string, int> ByMonth { get; set; } = [];
}

public sealed class EquipmentKpisDto
{
    public int Total { get; set; }
    public List<EquipmentFailureKpiDto> TopFailing { get; set; } = [];
}

public sealed class EquipmentFailureKpiDto
{
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int NoticeCount { get; set; }
}

public sealed class SyncKpisDto
{
    public int PendingSync { get; set; }
    public int SyncedLast24h { get; set; }
}
