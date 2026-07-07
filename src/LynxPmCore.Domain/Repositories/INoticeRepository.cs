using LynxPmCore.Domain.Aggregates.Notices;
using LynxPmCore.Domain.Enums;

namespace LynxPmCore.Domain.Repositories;

public interface INoticeRepository
{
    Task<Notice?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Notice?> GetByNumberAsync(string number, CancellationToken ct = default);
    Task<Notice?> GetByApexIdAsync(string apexId, CancellationToken ct = default);
    Task<(IReadOnlyList<Notice> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize,
        NoticeStatus? status = null,
        string? equipmentCode = null,
        string? createdBy = null,
        CancellationToken ct = default);
    Task<IReadOnlyList<Notice>> GetNotSynchronizedAsync(CancellationToken ct = default);
    Task AddAsync(Notice notice, CancellationToken ct = default);
    Task UpdateAsync(Notice notice, CancellationToken ct = default);
}
