using VitoriaAirlinesWeb.Data.Entities;
using VitoriaAirlinesWeb.Models.ViewModels.Dashboard;

namespace VitoriaAirlinesWeb.Services
{
    /// <summary>
    /// Defines the contract for a service that handles real-time notifications to various user groups.
    /// </summary>
    public interface INotificationService
    {
        /// <summary>
        /// Asynchronously sends a general message notification to all users in the "Admins" group.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <returns>Task: A Task representing the asynchronous operation.</returns>
        Task NotifyAdminsAsync(string message);



        /// <summary>
        /// Asynchronously sends a general message notification to all users in the "Employees" group.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <returns>Task: A Task representing the asynchronous operation.</returns>
        Task NotifyEmployeesAsync(string message);



        /// <summary>
        /// Asynchronously sends a general message notification to all users in the "Customers" group.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <returns>Task: A Task representing the asynchronous operation.</returns>
        Task NotifyCustomersAsync(string message);



        /// <summary>
        /// Asynchronously sends a message notification to all customers who have tickets for a specific flight.
        /// </summary>
        /// <param name="flight">The Flight entity whose customers are to be notified.</param>
        /// <param name="message">The message to send.</param>
        /// <returns>Task: A Task representing the asynchronous operation.</returns>
        Task NotifyFlightCustomersAsync(Flight flight, string message);



        /// <summary>
        /// Asynchronously notifies relevant groups (Admins, Employees) about a change in flight status.
        /// </summary>
        /// <param name="flightId">The ID of the flight whose status changed.</param>
        /// <param name="newStatus">The new status of the flight (e.g., "Departed", "Completed").</param>
        /// <returns>Task: A Task representing the asynchronous operation.</returns>
        Task NotifyFlightStatusChangedAsync(int flightId, string newStatus);



        /// <summary>
        /// Asynchronously notifies relevant groups (Admins, Employees) about a newly scheduled flight.
        /// </summary>
        /// <param name="flight">The FlightDashboardViewModel representing the new flight.</param>
        /// <returns>Task: A Task representing the asynchronous operation.</returns>
        Task NotifyNewFlightScheduledAsync(FlightDashboardViewModel flight);



        /// <summary>
        /// Asynchronously notifies relevant groups (Admins, Employees) about an updated flight on the dashboard.
        /// </summary>
        /// <param name="flight">The FlightDashboardViewModel representing the updated flight.</param>
        /// <returns>Task: A Task representing the asynchronous operation.</returns>
        Task NotifyUpdatedFlightDashboardAsync(FlightDashboardViewModel flight);



        /// <summary>
        /// Asynchronously sends a direct message notification to a specific user by their ID.
        /// </summary>
        /// <param name="userId">The ID of the user to notify.</param>
        /// <param name="message">The message to send.</param>
        /// <returns>Task: A Task representing the asynchronous operation.</returns>
        Task NotifyCustomerAsync(string userId, string message);


    }
}