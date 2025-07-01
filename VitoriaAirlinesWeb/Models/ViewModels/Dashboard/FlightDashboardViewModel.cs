namespace VitoriaAirlinesWeb.Models.ViewModels.Dashboard
{
    public class FlightDashboardViewModel
    {
        public int Id { get; set; }
        public string FlightNumber { get; set; } = null!;

        public string OriginAirportFullName { get; set; } = null!;
        public string OriginCountryFlagUrl { get; set; } = null!;

        public string DestinationAirportFullName { get; set; } = null!;
        public string DestinationCountryFlagUrl { get; set; } = null!;

        public string DepartureFormatted { get; set; } = null!;

    }
}
