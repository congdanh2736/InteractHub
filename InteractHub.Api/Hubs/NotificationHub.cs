using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace InteractHub.Api.Hubs
{
    [Authorize] 
    public class NotificationHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            Console.WriteLine($"User Connected: {Context.UserIdentifier} - ConnectionId: {Context.ConnectionId}");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            Console.WriteLine($"User Disconnected: {Context.UserIdentifier}");
            await base.OnDisconnectedAsync(exception);
        }
    }
}
