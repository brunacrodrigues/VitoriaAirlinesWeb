using VitoriaAirlinesWeb.Data.Entities;
using VitoriaAirlinesWeb.Models.ViewModels.Dashboard.VitoriaAirlinesWeb.Models.ViewModels.Dashboard;

namespace VitoriaAirlinesWeb.Models.ViewModels.Dashboard
{
    public class EmployeeDashboardViewModel
    {
        public int TotalScheduledFlights { get; set; }
        public int TotalCompletedFlights { get; set; }
        public int TotalTicketsSold { get; set; }
        public double AverageOccupancy { get; set; }

        public IEnumerable<FlightDashboardViewModel> ScheduledFlights { get; set; }
        public List<Flight> RecentFlights { get; set; }
        public List<LowOccupancyFlightViewModel> LowOccupancyFlights { get; set; }

    }
}
