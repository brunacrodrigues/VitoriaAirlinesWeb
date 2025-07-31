using System.ComponentModel.DataAnnotations;

namespace VitoriaAirlinesWeb.Models.ViewModels.Booking
{
    /// <summary>
    /// Represents the view model for confirming a single one-way flight booking.
    /// Includes flight and seat details, price, and customer/guest information.
    /// </summary>
    public class ConfirmBookingViewModel
    {
        /// <summary>
        /// Gets or sets the ID of the selected flight.
        /// </summary>
        public int FlightId { get; set; }


        /// <summary>
        /// Gets or sets the ID of the selected seat.
        /// </summary>
        public int SeatId { get; set; }


        /// <summary>
        /// Gets or sets the flight number.
        /// </summary>
        public string FlightNumber { get; set; } = null!;


        /// <summary>
        /// Gets or sets formatted information about the departure (e.g., "Airport Name (IATA)").
        /// </summary>
        public string DepartureInfo { get; set; } = null!;


        /// <summary>
        /// Gets or sets formatted information about the arrival (e.g., "Airport Name (IATA)").
        /// </summary>
        public string ArrivalInfo { get; set; } = null!;


        /// <summary>
        /// Gets or sets the local departure date and time.
        /// </summary>
        public DateTime DepartureTime { get; set; }


        /// <summary>
        /// Gets or sets the local arrival date and time.
        /// </summary>
        public DateTime ArrivalTime { get; set; }


        /// <summary>
        /// Gets or sets formatted information about the selected seat (e.g., "Row 12, Seat A").
        /// </summary>
        public string SeatInfo { get; set; } = null!;


        /// <summary>
        /// Gets or sets the class of the selected seat (e.g., "Economy", "Executive").
        /// </summary>
        public string SeatClass { get; set; } = null!;


        /// <summary>
        /// Gets or sets the final price for the booking.
        /// </summary>
        public decimal FinalPrice { get; set; }


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
        /// Gets or sets the email address provided for the booking. Nullable.
        /// </summary>
        public string? Email { get; set; }
    }
}