using VitoriaAirlinesWeb.Data.Entities;

namespace VitoriaAirlinesWeb.Models.ViewModels.Dashboard
{
    /// <summary>
    /// Represents the view model for an employee's dashboard, providing an overview of flight and ticket metrics.
    /// </summary>
    public class EmployeeDashboardViewModel
    {
        /// <summary>
        /// Gets or sets the total number of scheduled flights.
        /// </summary>
        public int TotalScheduledFlights { get; set; }


        /// <summary>
        /// Gets or sets the total number of completed flights.
        /// </summary>
        public int TotalCompletedFlights { get; set; }


        /// <summary>
        /// Gets or sets the total number of tickets sold (e.g., in the last 7 days).
        /// </summary>
        public int TotalTicketsSold { get; set; }


        /// <summary>
        /// Gets or sets the average occupancy rate across flights.
        /// </summary>
        public double AverageOccupancy { get; set; }


        /// <summary>
        /// Gets or sets a collection of scheduled flights relevant to the employee.
        /// </summary>
        public IEnumerable<FlightDashboardViewModel> ScheduledFlights { get; set; }


        /// <summary>
        /// Gets or sets a list of recent flights.
        /// </summary>
        public List<Flight> RecentFlights { get; set; }


        /// <summary>
        /// Gets or sets a list of upcoming flights with low occupancy.
        /// </summary>
        public List<LowOccupancyFlightViewModel> LowOccupancyFlights { get; set; }

    }
}