using System.ComponentModel.DataAnnotations;
using VitoriaAirlinesWeb.Data.Entities;
using VitoriaAirlinesWeb.Data.Enums;

namespace VitoriaAirlinesWeb.Models.ViewModels.Booking
{
    public class ConfirmRoundTripViewModel
    {
        public Flight OutboundFlight { get; set; } = null!;
        public Flight ReturnFlight { get; set; } = null!;
        public Seat OutboundSeat { get; set; } = null!;
        public Seat ReturnSeat { get; set; } = null!;

        public bool IsCustomer { get; set; }
        public string? ExistingPassportNumber { get; set; }

        [Display(Name = "Passport Number")]
        public string? PassportNumber { get; set; }


        public string? Email { get; set; }


        [Display(Name = "First Name")]
        public string? FirstName { get; set; }


        [Display(Name = "Last Name")]
        public string? LastName { get; set; }

        public decimal OutboundPrice =>
         OutboundSeat?.Class == SeatClass.Economy
             ? OutboundFlight?.EconomyClassPrice ?? 0
             : OutboundFlight?.ExecutiveClassPrice ?? 0;

        public decimal ReturnPrice =>
            ReturnSeat?.Class == SeatClass.Economy
                ? ReturnFlight?.EconomyClassPrice ?? 0
                : ReturnFlight?.ExecutiveClassPrice ?? 0;

        public decimal TotalPrice => OutboundPrice + ReturnPrice;

    }
}
