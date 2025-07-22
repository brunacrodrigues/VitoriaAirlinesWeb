namespace VitoriaAirlinesWeb.Models.ViewModels.Flights
{
    public class FlightDisplayViewModel
    {
        public int Id { get; set; }
        public string FlightNumber { get; set; } = null!;
        public string AirplaneModel { get; set; } = null!;
        public string Origin { get; set; } = null!;
        public string OriginFlagUrl { get; set; } = null!;
        public string Destination { get; set; } = null!;
        public string DestinationFlagUrl { get; set; } = null!;
        public string Departure { get; set; } = null!;
        public string Arrival { get; set; } = null!;
        public string Duration { get; set; } = null!;
        public decimal ExecutiveClassPrice { get; set; }
        public decimal EconomyClassPrice { get; set; }
        public string Status { get; set; } = null!;
    }
}
