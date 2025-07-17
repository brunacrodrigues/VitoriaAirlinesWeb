namespace VitoriaAirlinesWeb.Models.ViewModels.Dashboard
{
    public class CustomerDashboardViewModel
    {
        public int FlightsBooked { get; set; }
        public int FlightsCompleted { get; set; }
        public int FlightsCanceled { get; set; }
        public decimal TotalSpent { get; set; }

        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;

        public string? PassportNumber { get; set; }


        public string? Country { get; set; }
        public string? CountryFlagUrl { get; set; }


        public string? ProfilePictureUrl { get; set; }

        public List<FlightInfoViewModel> UpcomingFlights { get; set; }

        public List<FlightInfoViewModel> PastFlights { get; set; }

        public FlightInfoViewModel? LastCompletedFlight { get; set; }

    }
}
