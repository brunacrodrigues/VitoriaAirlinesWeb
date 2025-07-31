using Microsoft.AspNetCore.SignalR;
using VitoriaAirlinesWeb.Data.Entities;
using VitoriaAirlinesWeb.Hubs;
using VitoriaAirlinesWeb.Models.ViewModels.Dashboard;

namespace VitoriaAirlinesWeb.Services
{
    /// <summary>
    /// Provides services for sending real-time notifications to clients using SignalR.
    /// Notifications can be sent to specific user groups or individual users.
    /// </summary>
    public class NotificationService : INotificationService
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        /// <summary>
        /// Initializes a new instance of the NotificationService class.
        /// </summary>
        /// <param name="hubContext">The SignalR HubContext for sending messages to clients.</param>
        public NotificationService(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }


        /// <summary>
        /// Asynchronously sends a general message notification to all connected clients in the "Admins" group.
        /// </summary>
        /// <param name="message">The message content to send.</param>
        /// <returns>Task: A Task representing the asynchronous operation.</returns>
        public async Task NotifyAdminsAsync(string message)
        {
            await _hubContext.Clients.Group("Admins").SendAsync("ReceiveNotification", message);
        }


        /// <summary>
        /// Asynchronously sends a general message notification to all connected clients in the "Customers" group.
        /// </summary>
        /// <param name="message">The message content to send.</param>
        /// <returns>Task: A Task representing the asynchronous operation.</returns>
        public async Task NotifyCustomersAsync(string message)
        {
            await _hubContext.Clients.Group("Customers").SendAsync("ReceiveNotification", message);
        }


        /// <summary>
        /// Asynchronously sends a general message notification to all connected clients in the "Employees" group.
        /// </summary>
        /// <param name="message">The message content to send.</param>
        /// <returns>Task: A Task representing the asynchronous operation.</returns>
        public async Task NotifyEmployeesAsync(string message)
        {
            await _hubContext.Clients.Group("Employees").SendAsync("ReceiveNotification", message);
        }


        /// <summary>
        /// Asynchronously sends a message notification to all individual customers associated with a specific flight.
        /// </summary>
        /// <param name="flight">The Flight entity containing the tickets (and thus UserIds) of customers to notify.</param>
        /// <param name="message">The message content to send.</param>
        /// <returns>Task: A Task representing the asynchronous operation.</returns>
        public async Task NotifyFlightCustomersAsync(Flight flight, string message)
        {
            // Ensure flight.Tickets is not null before iterating.
            if (flight.Tickets is null) return;

            foreach (var ticket in flight.Tickets)
            {
                // Send notification to the specific user ID associated with the ticket.
                await _hubContext.Clients.User(ticket.UserId).SendAsync("ReceiveNotification", message);
            }
        }


        /// <summary>
        /// Asynchronously notifies relevant dashboard groups (Admins, Employees) about a change in a flight's status.
        /// </summary>
        /// <param name="flightId">The ID of the flight whose status changed.</param>
        /// <param name="newStatus">A string representing the new status of the flight (e.g., "Departed", "Completed").</param>
        /// <returns>Task: A Task representing the asynchronous operation.</returns>
        public async Task NotifyFlightStatusChangedAsync(int flightId, string newStatus)
        {
            // Send notification to both Admin and Employee groups.
            await _hubContext.Clients.Group("Admins").SendAsync("FlightStatusChanged", flightId, newStatus);
            await _hubContext.Clients.Group("Employees").SendAsync("FlightStatusChanged", flightId, newStatus);
        }


        /// <summary>
        /// Asynchronously notifies relevant dashboard groups (Admins, Employees) about a newly scheduled flight.
        /// </summary>
        /// <param name="flight">The FlightDashboardViewModel containing details of the new flight.</param>
        /// <returns>Task: A Task representing the asynchronous operation.</returns>
        public async Task NotifyNewFlightScheduledAsync(FlightDashboardViewModel flight)
        {
            // Send notification to both Admin and Employee groups.
            await _hubContext.Clients.Group("Admins").SendAsync("NewFlightScheduled", flight);
            await _hubContext.Clients.Group("Employees").SendAsync("NewFlightScheduled", flight);
        }


        /// <summary>
        /// Asynchronously notifies relevant dashboard groups (Admins, Employees) about an updated flight.
        /// </summary>
        /// <param name="flight">The FlightDashboardViewModel containing details of the updated flight.</param>
        /// <returns>Task: A Task representing the asynchronous operation.</returns>
        public async Task NotifyUpdatedFlightDashboardAsync(FlightDashboardViewModel flight)
        {
            // Send notification to both Admin and Employee groups.
            await _hubContext.Clients.Group("Admins").SendAsync("UpdatedFlight", flight);
            await _hubContext.Clients.Group("Employees").SendAsync("UpdatedFlight", flight);
        }


        /// <summary>
        /// Asynchronously sends a direct message notification to a specific user identified by their ID.
        /// </summary>
        /// <param name="userId">The ID of the user to notify.</param>
        /// <param name="message">The message content to send.</param>
        /// <returns>Task: A Task representing the asynchronous operation.</returns>
        public async Task NotifyCustomerAsync(string userId, string message)
        {
            await _hubContext.Clients.User(userId)
                .SendAsync("ReceiveNotification", message);
        }
    }
}