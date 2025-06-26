using VitoriaAirlinesWeb.Data.Entities;

namespace VitoriaAirlinesWeb.Models.ViewModels.Booking
{
    public class SelectSeatViewModel
    {
        public int FlightId { get; set; }

        public Airport OriginAirport { get; set; }
        public Airport DestinationAirport { get; set; }


        public string FlightInfo { get; set; }

        public decimal EconomyPrice { get; set; }


        public decimal ExecutivePrice { get; set; }

        public List<Seat> Seats { get; set; }


        public HashSet<int> OccupiedSeatsIds { get; set; }
    }
}
