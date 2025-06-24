using System.ComponentModel.DataAnnotations;

namespace VitoriaAirlinesWeb.Data.Entities
{
    public class Ticket : IEntity
    {
        public int Id { get; set; }

        [Required]
        public int FlightId { get; set; }

        public Flight Flight { get; set; }


        [Required]
        public int SeatId { get; set; }

        public Seat Seat { get; set; }


        [Required]
        public string UserId { get; set; }
        public User User { get; set; }


        [Required]
        public decimal PricePaid { get; set; }


        [Required]
        public DateTime PurchaseDateUtc { get; set; }
    }
}
