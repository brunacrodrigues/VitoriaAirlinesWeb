namespace VitoriaAirlinesWeb.Models.ViewModels.Tickets
{
    /// <summary>
    /// Represents a view model for displaying ticket information in lists (e.g., flight history).
    /// </summary>
    public class TicketDisplayViewModel
    {
        /// <summary>
        /// Gets or sets the unique identifier of the ticket.
        /// </summary>
        public int Id { get; set; }


        /// <summary>
        /// Gets or sets the flight number associated with the ticket.
        /// </summary>
        public string FlightNumber { get; set; } = null!;


        /// <summary>
        /// Gets or sets the full name of the origin airport.
        /// </summary>
        public string Origin { get; set; } = null!;


        /// <summary>
        /// Gets or sets the URL to the origin country's flag image.
        /// </summary>
        public string OriginFlagUrl { get; set; } = null!;


        /// <summary>
        /// Gets or sets the full name of the destination airport.
        /// </summary>
        public string Destination { get; set; } = null!;


        /// <summary>
        /// Gets or sets the URL to the destination country's flag image.
        /// </summary>
        public string DestinationFlagUrl { get; set; } = null!;


        /// <summary>
        /// Gets or sets the local departure date and time.
        /// </summary>
        public DateTime Departure { get; set; }


        /// <summary>
        /// Gets or sets the local arrival date and time.
        /// </summary>
        public DateTime Arrival { get; set; }


        /// <summary>
        /// Gets or sets the formatted display of the seat (e.g., "12A").
        /// </summary>
        public string SeatDisplay { get; set; } = null!;


        /// <summary>
        /// Gets or sets the class of the seat (e.g., "Economy", "Executive").
        /// </summary>
        public string Class { get; set; } = null!;


        /// <summary>
        /// Gets or sets the price paid for the ticket.
        /// </summary>
        public decimal PricePaid { get; set; }


        /// <summary>
        /// Gets or sets the status of the flight associated with the ticket.
        /// </summary>
        public string Status { get; set; } = null!;
    }
}