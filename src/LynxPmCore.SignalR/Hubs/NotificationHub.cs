using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace LynxPmCore.SignalR.Hubs;

[Authorize]
public sealed class NotificationHub : Hub
{
    public async Task JoinGroup(string group)
        => await Groups.AddToGroupAsync(Context.ConnectionId, group);

    public async Task LeaveGroup(string group)
        => await Groups.RemoveFromGroupAsync(Context.ConnectionId, group);
}
