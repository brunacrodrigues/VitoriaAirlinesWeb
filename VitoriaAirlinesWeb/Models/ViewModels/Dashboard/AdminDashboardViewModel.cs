namespace VitoriaAirlinesWeb.Models.ViewModels.Dashboard
{
    public class AdminDashboardViewModel
    {
        public int TotalFlights { get; set; }
        public int TotalTickets { get; set; }
        public int TotalEmployees { get; set; }
        public int TotalAirplanes { get; set; }
        public int TotalCustomers { get; set; }

        public decimal TotalRevenue { get; set; }

        public double AverageOccupancyRate { get; set; }

        public IEnumerable<FlightDashboardViewModel> ScheduledFlights { get; set; }

        public List<TicketSalesByDayViewModel> TicketSalesLast7Days { get; set; }
        public List<AirplaneOccupancyViewModel> AirplaneOccupancyStats { get; set; }
        public List<TopDestinationViewModel> TopDestinations { get; set; }

        public MostActiveAirplaneViewModel? MostActiveAirplane { get; set; }

        public LeastOccupiedAirplaneViewModel? LeastOccupiedAirplane { get; set; }
    }
}
