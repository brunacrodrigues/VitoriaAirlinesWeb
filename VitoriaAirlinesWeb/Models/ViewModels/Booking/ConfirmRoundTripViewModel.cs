using System.ComponentModel.DataAnnotations;
using VitoriaAirlinesWeb.Data.Entities;
using VitoriaAirlinesWeb.Data.Enums;

namespace VitoriaAirlinesWeb.Models.ViewModels.Booking
{
    /// <summary>
    /// Represents the view model for confirming a round-trip flight booking.
    /// Includes details for both outbound and return flights and their selected seats,
    /// along with guest/customer information.
    /// </summary>
    public class ConfirmRoundTripViewModel
    {

        public int OutboundFlightId { get; set; }
        public int OutboundSeatId { get; set; }
        public int ReturnFlightId { get; set; }
        public int ReturnSeatId { get; set; }

        /// <summary>
        /// Gets or sets the outbound Flight entity.
        /// </summary>
        public Flight OutboundFlight { get; set; } = null!;
        /// <summary>
        /// Gets or sets the selected Seat for the outbound flight.
        /// </summary>
        public Seat OutboundSeat { get; set; } = null!;

        /// <summary>
        /// Gets or sets the return Flight entity.
        /// </summary>
        public Flight ReturnFlight { get; set; } = null!;
        /// <summary>
        /// Gets or sets the selected Seat for the return flight.
        /// </summary>
        public Seat ReturnSeat { get; set; } = null!;

        /// <summary>
        /// Gets or sets a value indicating whether the booking is for a registered customer (true) or a guest (false).
        /// </summary>
        public bool IsCustomer { get; set; }
        /// <summary>
        /// Gets or sets the existing passport number of the customer, if available. Nullable.
        /// </summary>
        public string? ExistingPassportNumber { get; set; }

        /// <summary>
        /// Gets or sets the passport number provided for the booking. Nullable.
        /// </summary>
        [Display(Name = "Passport Number")]
        public string? PassportNumber { get; set; }


        /// <summary>
        /// Gets or sets the email address provided for the booking. Nullable.
        /// </summary>
        public string? Email { get; set; }


        /// <summary>
        /// Gets or sets the first name provided for the booking. Nullable.
        /// </summary>
        [Display(Name = "First Name")]
        public string? FirstName { get; set; }


        /// <summary>
        /// Gets or sets the last name provided for the booking. Nullable.
        /// </summary>
        [Display(Name = "Last Name")]
        public string? LastName { get; set; }

        /// <summary>
        /// Gets the calculated price for the outbound flight based on the selected seat class.
        /// </summary>
        public decimal OutboundPrice =>
         OutboundSeat?.Class == SeatClass.Economy
             ? OutboundFlight?.EconomyClassPrice ?? 0
             : OutboundFlight?.ExecutiveClassPrice ?? 0;

        /// <summary>
        /// Gets the calculated price for the return flight based on the selected seat class.
        /// </summary>
        public decimal ReturnPrice =>
            ReturnSeat?.Class == SeatClass.Economy
                ? ReturnFlight?.EconomyClassPrice ?? 0
                : ReturnFlight?.ExecutiveClassPrice ?? 0;

        /// <summary>
        /// Gets the total price for the round trip (OutboundPrice + ReturnPrice).
        /// </summary>
        public decimal TotalPrice => OutboundPrice + ReturnPrice;

    }
}