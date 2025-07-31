using VitoriaAirlinesWeb.Data.Entities;

namespace VitoriaAirlinesWeb.Models.ViewModels.Booking
{
    /// <summary>
    /// Represents the view model for selecting a seat for a flight.
    /// Includes flight details, seat availability, and pricing.
    /// </summary>
    public class SelectSeatViewModel
    {
        /// <summary>
        /// Gets or sets the ID of the flight.
        /// </summary>
        public int FlightId { get; set; }


        /// <summary>
        /// Gets or sets the origin Airport entity.
        /// </summary>
        public Airport OriginAirport { get; set; }


        /// <summary>
        /// Gets or sets the destination Airport entity.
        /// </summary>
        public Airport DestinationAirport { get; set; }


        /// <summary>
        /// Gets or sets formatted information about the flight.
        /// </summary>
        public string FlightInfo { get; set; } = null!;


        /// <summary>
        /// Gets or sets the economy class price for the flight.
        /// </summary>
        public decimal EconomyPrice { get; set; }


        /// <summary>
        /// Gets or sets the executive class price for the flight.
        /// </summary>
        public decimal ExecutivePrice { get; set; }


        /// <summary>
        /// Gets or sets the list of all seats available on the airplane for this flight.
        /// </summary>
        public List<Seat> Seats { get; set; }


        /// <summary>
        /// Gets or sets a hash set containing the IDs of seats that are already occupied for this flight.
        /// </summary>
        public HashSet<int> OccupiedSeatsIds { get; set; }


        /// <summary>
        /// Gets or sets a value indicating whether the seat selection is for changing an existing ticket's seat.
        /// </summary>
        public bool IsChangingSeat { get; set; }


        /// <summary>
        /// Gets or sets the ID of the ticket if a seat is being changed. Nullable.
        /// </summary>
        public int? TicketId { get; set; }

    }
}