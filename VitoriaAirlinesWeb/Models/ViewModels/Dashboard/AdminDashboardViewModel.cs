namespace VitoriaAirlinesWeb.Models.ViewModels.Dashboard
{
    /// <summary>
    /// Represents the view model for the administrator's dashboard, providing an overview of key system metrics.
    /// </summary>
    public class AdminDashboardViewModel
    {
        /// <summary>
        /// Gets or sets the total number of flights in the system.
        /// </summary>
        public int TotalFlights { get; set; }


        /// <summary>
        /// Gets or sets the total number of tickets sold.
        /// </summary>
        public int TotalTickets { get; set; }


        /// <summary>
        /// Gets or sets the total number of registered employees.
        /// </summary>
        public int TotalEmployees { get; set; }


        /// <summary>
        /// Gets or sets the total number of active airplanes.
        /// </summary>
        public int TotalAirplanes { get; set; }


        /// <summary>
        /// Gets or sets the total number of registered customers.
        /// </summary>
        public int TotalCustomers { get; set; }


        /// <summary>
        /// Gets or sets the total revenue generated.
        /// </summary>
        public decimal TotalRevenue { get; set; }


        /// <summary>
        /// Gets or sets the average occupancy rate across all flights.
        /// </summary>
        public double AverageOccupancyRate { get; set; }


        /// <summary>
        /// Gets or sets a collection of scheduled flights to display on the dashboard.
        /// </summary>
        public IEnumerable<FlightDashboardViewModel> ScheduledFlights { get; set; } = null!;


        /// <summary>
        /// Gets or sets a list of ticket sales data for the last 7 days.
        /// </summary>
        public List<TicketSalesByDayViewModel> TicketSalesLast7Days { get; set; } = null!;


        /// <summary>
        /// Gets or sets a list of occupancy statistics for various airplane models.
        /// </summary>
        public List<AirplaneOccupancyViewModel> AirplaneOccupancyStats { get; set; } = null!;


        /// <summary>
        /// Gets or sets a list of top destination airports based on ticket sales.
        /// </summary>
        public List<TopDestinationViewModel> TopDestinations { get; set; } = null!;


        /// <summary>
        /// Gets or sets the most active airplane model (e.g., based on flight count). Nullable.
        /// </summary>
        public MostActiveAirplaneViewModel? MostActiveAirplane { get; set; }


        /// <summary>
        /// Gets or sets the least occupied airplane model (e.g., based on occupancy rate). Nullable.
        /// </summary>
        public LeastOccupiedAirplaneViewModel? LeastOccupiedAirplane { get; set; }
    }
}