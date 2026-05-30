using LynxPmCore.Application.Common.Interfaces;
using LynxPmCore.SignalR.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace LynxPmCore.SignalR.Services;

internal sealed class SignalRNotificationService(IHubContext<NotificationHub> hubContext) : INotificationService
{
    public async Task SendToUserAsync(string userId, string method, object payload, CancellationToken ct = default)
        => await hubContext.Clients.User(userId).SendAsync(method, payload, ct);

    public async Task SendToGroupAsync(string group, string method, object payload, CancellationToken ct = default)
        => await hubContext.Clients.Group(group).SendAsync(method, payload, ct);

    public async Task BroadcastAsync(string method, object payload, CancellationToken ct = default)
        => await hubContext.Clients.All.SendAsync(method, payload, ct);
}
