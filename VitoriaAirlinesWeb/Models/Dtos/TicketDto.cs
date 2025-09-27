namespace VitoriaAirlinesWeb.Models.Dtos
{
    /// <summary>
    /// Represents a Data Transfer Object (DTO) for a flight ticket, typically used for displaying ticket information.
    /// </summary>
    public class TicketDto
    {
        /// <summary>
        /// Gets or sets the unique identifier of the ticket.
        /// </summary>
        public int TicketId { get; set; }


        /// <summary>
        /// Gets or sets the unique identifier of the flight associated with the ticket.
        /// </summary>
        public int FlightId { get; set; }


        /// <summary>
        /// Gets or sets the flight number associated with the ticket.
        /// </summary>
        public string FlightNumber { get; set; } = null!;


        /// <summary>
        /// Gets or sets the departure date and time of the flight in UTC.
        /// </summary>
        public DateTime DepartureUtc { get; set; }


        /// <summary>
        /// Gets or sets the arrival date and time of the flight in UTC.
        /// </summary>
        public DateTime ArrivalUtc { get; set; }


        /// <summary>
        /// Gets or sets the full name of the origin airport.
        /// </summary>
        public string OriginAirport { get; set; } = null!;


        /// <summary>
        /// Gets or sets the full name of the destination airport.
        /// </summary>
        public string DestinationAirport { get; set; } = null!;


        public string OriginCountryCode { get; set; } = string.Empty;
        public string DestinationCountryCode { get; set; } = string.Empty;


        /// <summary>
        /// Gets or sets the formatted seat number (e.g., "12A Economy").
        /// </summary>
        public string Seat { get; set; } = null!;


        /// <summary>
        /// Gets or sets the price paid for the ticket.
        /// </summary>
        public decimal PricePaid { get; set; }


        /// <summary>
        /// Gets or sets the date and time when the ticket was purchased in UTC.
        /// </summary>
        public DateTime PurchaseDateUtc { get; set; }


        /// <summary>
        /// Gets or sets the status of the flight associated with the ticket.
        /// </summary>
        public string Status { get; set; } = null!;


    }
}