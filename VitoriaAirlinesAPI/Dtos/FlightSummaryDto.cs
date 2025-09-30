namespace VitoriaAirlinesAPI.Dtos
{
    public class FlightSummaryDto
    {
        public string FlightNumber { get; set; } = string.Empty;
        public DateTime DepartureTime { get; set; }
        public string OriginIATA { get; set; } = string.Empty;
        public string DestinationIATA { get; set; } = string.Empty;
        public string OriginAirportFullName { get; set; } = string.Empty;
        public string DestinationAirportFullName { get; set; } = string.Empty;
        public string SeatNumber { get; set; } = string.Empty;

        public string OriginCountryCode { get; set; } = string.Empty;
        public string DestinationCountryCode { get; set; } = string.Empty;

    }
}
