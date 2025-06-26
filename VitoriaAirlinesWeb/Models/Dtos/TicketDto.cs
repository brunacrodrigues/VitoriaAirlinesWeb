namespace VitoriaAirlinesWeb.Models.Dtos
{
    public class TicketDto
    {
        public int TicketId { get; set; }
        public string FlightNumber { get; set; } = null!;
        public DateTime DepartureUtc { get; set; }
        public string OriginAirport { get; set; } = null!;
        public string DestinationAirport { get; set; } = null!;
        public string Seat { get; set; } = null!;
        public decimal PricePaid { get; set; }
        public DateTime PurchaseDateUtc { get; set; }
    }
}
