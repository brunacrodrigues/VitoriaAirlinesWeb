using VitoriaAirlinesWeb.Data.Entities;
using VitoriaAirlinesWeb.Models.ViewModels.Dashboard;

namespace VitoriaAirlinesWeb.Services
{
    public interface INotificationService
    {
        Task NotifyAdminsAsync(string message);

        Task NotifyEmployeesAsync(string message);

        Task NotifyCustomersAsync(string message);

        Task NotifyFlightCustomersAsync(Flight flight, string message);

        Task NotifyFlightStatusChangedAsync(int flightId, string newStatus);

        Task NotifyNewFlightScheduledAsync(FlightDashboardViewModel flight);


    }
}
