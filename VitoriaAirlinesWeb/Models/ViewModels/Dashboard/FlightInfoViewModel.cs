namespace VitoriaAirlinesWeb.Models.ViewModels.Dashboard
{
    public class FlightInfoViewModel
    {
        public string FlightNumber { get; set; } = null!;
        public string OriginAirport { get; set; } = null!;
        public string OriginCountryFlagUrl { get; set; } = null!;
        public string DestinationAirport { get; set; } = null!;
        public string DestinationCountryFlagUrl { get; set; } = null!;
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
    }
}
