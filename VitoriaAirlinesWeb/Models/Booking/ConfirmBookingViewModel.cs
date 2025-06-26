using System.ComponentModel.DataAnnotations;

namespace VitoriaAirlinesWeb.Models.Booking
{
    public class ConfirmBookingViewModel
    {
        public int FlightId { get; set; }

        public int SeatId { get; set; }

        public string FlightNumber { get; set; } = null!;


        public string DepartureInfo { get; set; } = null!;


        public string ArrivalInfo { get; set; } = null!;


        public DateTime DepartureTime { get; set; }


        public DateTime ArrivalTime { get; set; }


        public string SeatInfo { get; set; } = null!;


        public string SeatClass { get; set; } = null!;


        public decimal FinalPrice { get; set; }

        public bool IsCustomer {  get; set; }


        [Display(Name = "First Name")]
        [Required(ErrorMessage = "First Name is required")]
        public string? FirstName { get; set; }


        [Display(Name = "Last Name")]
        [Required(ErrorMessage = "Last Name is required")]
        public string? LastName { get; set; }


        [Required(ErrorMessage = "Email is required")]
        [DataType(DataType.EmailAddress)]
        public string? Email { get; set; }
    }
}
