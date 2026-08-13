using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace OPS.API.Hubs;

[Authorize]
public class OperationsHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        // Hub automatically maps connections to User ID if ClaimTypes.NameIdentifier is present.
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
    }
}
