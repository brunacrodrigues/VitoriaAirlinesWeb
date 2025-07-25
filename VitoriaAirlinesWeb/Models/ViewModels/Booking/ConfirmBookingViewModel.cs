using System.ComponentModel.DataAnnotations;

namespace VitoriaAirlinesWeb.Models.ViewModels.Booking
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

        public bool IsCustomer { get; set; }

        public string? ExistingPassportNumber { get; set; }


        [Display(Name = "Passport Number")]
        public string? PassportNumber { get; set; }


        [Display(Name = "First Name")]
        public string? FirstName { get; set; }


        [Display(Name = "Last Name")]
        public string? LastName { get; set; }



        public string? Email { get; set; }
    }
}
