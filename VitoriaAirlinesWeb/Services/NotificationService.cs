
using Microsoft.AspNetCore.SignalR;
using VitoriaAirlinesWeb.Data.Entities;
using VitoriaAirlinesWeb.Hubs;
using VitoriaAirlinesWeb.Models.ViewModels.Dashboard;

namespace VitoriaAirlinesWeb.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationService(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }


        public async Task NotifyAdminsAsync(string message)
        {
            await _hubContext.Clients.Group("Admins").SendAsync("ReceiveNotification", message);
        }


        public async Task NotifyCustomersAsync(string message)
        {
            await _hubContext.Clients.Group("Customers").SendAsync("ReceiveNotification", message);
        }


        public async Task NotifyEmployeesAsync(string message)
        {
            await _hubContext.Clients.Group("Employees").SendAsync("ReceiveNotification", message);
        }


        public async Task NotifyFlightCustomersAsync(Flight flight, string message)
        {
            if (flight.Tickets is null) return;


            foreach (var ticket in flight.Tickets)
            {
                await _hubContext.Clients.User(ticket.UserId).SendAsync("ReceiveNotification", message);
            }
        }

        public async Task NotifyFlightStatusChangedAsync(int flightId, string newStatus)
        {
            await _hubContext.Clients.Group("Admins").SendAsync("FlightStatusChanged", flightId, newStatus);
            await _hubContext.Clients.Group("Employees").SendAsync("FlightStatusChanged", flightId, newStatus);
        }

        public async Task NotifyNewFlightScheduledAsync(FlightDashboardViewModel flight)
        {
            await _hubContext.Clients.Group("Admins").SendAsync("NewFlightScheduled", flight);
            await _hubContext.Clients.Group("Employees").SendAsync("NewFlightScheduled", flight);
        }

        public async Task NotifyUpdatedFlightDashboardAsync(FlightDashboardViewModel flight)
        {
            await _hubContext.Clients.Group("Admins").SendAsync("UpdatedFlight", flight);
            await _hubContext.Clients.Group("Employees").SendAsync("UpdatedFlight", flight);
        }
    }
}
