using LynxPmCore.Application.Common.Interfaces;

namespace LynxPmCore.Mcp.Services;

internal sealed class NoopNotificationService : INotificationService
{
    public Task SendToUserAsync(string userId, string method, object payload, CancellationToken ct = default)
        => Task.CompletedTask;

    public Task SendToGroupAsync(string group, string method, object payload, CancellationToken ct = default)
        => Task.CompletedTask;

    public Task BroadcastAsync(string method, object payload, CancellationToken ct = default)
        => Task.CompletedTask;
}
