using System.ComponentModel.DataAnnotations;

namespace VitoriaAirlinesWeb.Data
{
    public class BookingLegDto
    {
        [Required]
        public int FlightId { get; set; }

        [Required]
        public int SeatId { get; set; }
    }
}
