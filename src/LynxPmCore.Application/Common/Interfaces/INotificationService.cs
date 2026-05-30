namespace LynxPmCore.Application.Common.Interfaces;

public interface INotificationService
{
    Task SendToUserAsync(string userId, string method, object payload, CancellationToken ct = default);
    Task SendToGroupAsync(string group, string method, object payload, CancellationToken ct = default);
    Task BroadcastAsync(string method, object payload, CancellationToken ct = default);
}
