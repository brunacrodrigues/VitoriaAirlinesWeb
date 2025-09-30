namespace VitoriaAirlinesAPI.Dtos
{
    public class BoardingPassDto
    {
        public int TicketId { get; set; }
        public string PassengerName { get; set; }
        public string FlightNumber { get; set; }
        public string FromAirportIATA { get; set; }
        public string FromAirportFullName { get; set; }
        public string ToAirportIATA { get; set; }
        public string ToAirportFullName { get; set; }
        public DateTime DepartureUtc { get; set; }
        public DateTime ArrivalUtc { get; set; }
        public string Gate { get; set; } = "N17";
        public string BoardingZone { get; set; } = "A";
        public string SeatNumber { get; set; }
        public string SeatClass { get; set; }
        public string TicketBarcodeValue { get; set; }
    }
}
