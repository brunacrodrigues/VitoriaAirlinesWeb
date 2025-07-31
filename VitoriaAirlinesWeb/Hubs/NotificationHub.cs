using Microsoft.AspNetCore.SignalR;
using VitoriaAirlinesWeb.Helpers;

namespace VitoriaAirlinesWeb.Hubs
{
    /// <summary>
    /// A SignalR Hub for handling real-time notifications to different user roles (Admins, Employees, Customers).
    /// Users are added to specific groups upon connection based on their authenticated roles.
    /// </summary>
    public class NotificationHub : Hub
    {
        /// <summary>
        /// Overrides the default OnConnectedAsync method to assign connecting users to SignalR groups
        /// based on their authenticated roles (Admin, Employee, Customer).
        /// </summary>
        /// <returns>Task: A Task representing the asynchronous operation.</returns>
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