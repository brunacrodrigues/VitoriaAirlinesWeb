using Microsoft.AspNetCore.SignalR;
using VitoriaAirlinesWeb.Helpers;

namespace VitoriaAirlinesWeb.Hubs
{
    public class NotificationHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var user = Context.User;

            if (user.Identity.IsAuthenticated)
            {
                if (user.IsInRole(UserRoles.Admin))
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, "Admins");

                }

                if (user.IsInRole(UserRoles.Employee))
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, "Employees");

                }

                if (user.IsInRole(UserRoles.Customer))
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, "Customers");

                }
            }

            await base.OnConnectedAsync();
        }
    }
}
