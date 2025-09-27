namespace VitoriaAirlinesAPI.Dtos
{
    public class FlightSearchResultDto
    {
        public int FlightId { get; set; }
        public string FlightNumber { get; set; } = null!;
        public string OriginAirportIATA { get; set; } = null!;
        public string OriginAirportFullName { get; set; } = null!;
        public string OriginCountryCode { get; set; } = null!;
        public string DestinationAirportIATA { get; set; } = null!;
        public string DestinationAirportFullName { get; set; } = null!;
        public string DestinationCountryCode { get; set; } = null!;
        public DateTime DepartureUtc { get; set; }
        public DateTime ArrivalUtc { get; set; }
        public TimeSpan Duration { get; set; }
        public decimal EconomyClassPrice { get; set; }
        public decimal ExecutiveClassPrice { get; set; }
        public int AvailableSeats { get; set; }
        public string Status { get; set; } = null!;
    }
}
