using Microsoft.AspNetCore.SignalR;

namespace AJAI_Server.Hubs
{
    public class DashboardHub : Hub
    {
        public async Task JoinDashboard()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "Admins");
        }

        public async Task JoinCameraGroup(string cameraEmail)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Camera_{cameraEmail}");
        }
    }
}