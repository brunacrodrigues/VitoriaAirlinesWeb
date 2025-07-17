namespace VitoriaAirlinesWeb.Models.ViewModels.Dashboard
{
    namespace VitoriaAirlinesWeb.Models.ViewModels.Dashboard
    {
        public class LowOccupancyFlightViewModel
        {
            public int Id { get; set; }
            public string FlightNumber { get; set; } = null!;

            public string OriginAirportFullName { get; set; } = null!;
            public string OriginCountryFlagUrl { get; set; } = null!;

            public string DestinationAirportFullName { get; set; } = null!;
            public string DestinationCountryFlagUrl { get; set; } = null!;

            public string DepartureFormatted { get; set; } = null!;
            public double OccupancyRate { get; set; }
        }
    }


}
