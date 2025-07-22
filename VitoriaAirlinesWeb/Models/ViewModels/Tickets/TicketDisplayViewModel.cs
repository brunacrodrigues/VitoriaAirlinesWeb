namespace VitoriaAirlinesWeb.Models.ViewModels.Tickets
{
    public class TicketDisplayViewModel
    {
        public int Id { get; set; }
        public string FlightNumber { get; set; } = null!;
        public string Origin { get; set; } = null!;
        public string OriginFlagUrl { get; set; } = null!;
        public string Destination { get; set; } = null!;
        public string DestinationFlagUrl { get; set; } = null!;
        public DateTime Departure { get; set; }
        public DateTime Arrival { get; set; }
        public string SeatDisplay { get; set; } = null!;
        public string Class { get; set; } = null!;
        public decimal PricePaid { get; set; }
        public string Status { get; set; } = null!;
    }
}
