namespace VitoriaAirlinesWeb.Models.ViewModels.Booking
{
    public class ConfirmSeatChangeViewModel
    {
        public int OldTicketId { get; set; }
        public int NewSeatId { get; set; }

        public string FlightNumber { get; set; } = null!;
        public string DepartureInfo { get; set; } = null!;
        public string ArrivalInfo { get; set; } = null!;
        public DateTime DepartureTime { get; set; }

        public DateTime ArrivalTime { get; set; }

        public string OldSeatInfo { get; set; } = null!;
        public string OldSeatClass { get; set; } = null!;
        public string NewSeatInfo { get; set; } = null!;
        public string NewSeatClass { get; set; } = null!;

        
        public decimal OldPricePaid { get; set; }
        public decimal NewPrice { get; set; }
        public decimal PriceDifference { get; set; }
    }
}
