namespace VitoriaAirlinesWeb.Models.ViewModels.Booking
{
    /// <summary>
    /// Represents the view model for confirming a seat change for an existing ticket.
    /// </summary>
    public class ConfirmSeatChangeViewModel
    {
        /// <summary>
        /// Gets or sets the ID of the old ticket.
        /// </summary>
        public int OldTicketId { get; set; }


        /// <summary>
        /// Gets or sets the ID of the newly selected seat.
        /// </summary>
        public int NewSeatId { get; set; }


        /// <summary>
        /// Gets or sets the flight number.
        /// </summary>
        public string FlightNumber { get; set; } = null!;


        /// <summary>
        /// Gets or sets formatted information about the departure.
        /// </summary>
        public string DepartureInfo { get; set; } = null!;


        /// <summary>
        /// Gets or sets formatted information about the arrival.
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
        /// Gets or sets formatted information about the old seat (e.g., "12A").
        /// </summary>
        public string OldSeatInfo { get; set; } = null!;


        /// <summary>
        /// Gets or sets the class of the old seat (e.g., "Economy").
        /// </summary>
        public string OldSeatClass { get; set; } = null!;


        /// <summary>
        /// Gets or sets formatted information about the new seat (e.g., "15B").
        /// </summary>
        public string NewSeatInfo { get; set; } = null!;


        /// <summary>
        /// Gets or sets the class of the new seat (e.g., "Executive").
        /// </summary>
        public string NewSeatClass { get; set; } = null!;


        /// <summary>
        /// Gets or sets the price originally paid for the old ticket.
        /// </summary>
        public decimal OldPricePaid { get; set; }


        /// <summary>
        /// Gets or sets the new price for the ticket with the selected new seat.
        /// </summary>
        public decimal NewPrice { get; set; }


        /// <summary>
        /// Gets or sets the difference between the new price and the old price (can be positive or negative).
        /// </summary>
        public decimal PriceDifference { get; set; }
    }
}