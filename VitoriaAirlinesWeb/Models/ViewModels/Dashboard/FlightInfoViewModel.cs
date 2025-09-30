namespace VitoriaAirlinesWeb.Models.ViewModels.Dashboard
{
    /// <summary>
    /// Represents a view model for displaying essential flight information, often used in lists or detailed views.
    /// </summary>
    public class FlightInfoViewModel
    {
        /// <summary>
        /// Gets or sets the flight number.
        /// </summary>
        public string FlightNumber { get; set; } = null!;


        /// <summary>
        /// Gets or sets the full name of the origin airport.
        /// </summary>
        public string OriginAirport { get; set; } = null!;

        /// <summary>
        /// Gets or sets the IATA code of the origin airport (e.g., "LIS").
        /// </summary>
        public string OriginAirportIATA { get; set; } = null!; // <--- ADICIONE ESTE

        /// <summary>
        /// Gets or sets the IATA code of the destination airport (e.g., "LHR").
        /// </summary>
        public string DestinationAirportIATA { get; set; } = null!;


        /// <summary>
        /// Gets or sets the URL to the origin country's flag image.
        /// </summary>
        public string OriginCountryFlagUrl { get; set; } = null!;

        public string OriginCountryCode { get; set; } = null!;


        public string DestinationCountryCode { get; set; } = null!;


        /// <summary>
        /// Gets or sets the full name of the destination airport.
        /// </summary>
        public string DestinationAirport { get; set; } = null!;


        /// <summary>
        /// Gets or sets the URL to the destination country's flag image.
        /// </summary>
        public string DestinationCountryFlagUrl { get; set; } = null!;


        /// <summary>
        /// Gets or sets the local departure date and time.
        /// </summary>
        public DateTime DepartureTime { get; set; }


        /// <summary>
        /// Gets or sets the local arrival date and time.
        /// </summary>
        public DateTime ArrivalTime { get; set; }


        /// <summary>
        /// Gets or sets the formatted seat number (e.g., "12A Economy"). Nullable.
        /// </summary>
        public string? SeatNumber { get; set; }


        /// <summary>
        /// Gets or sets the unique identifier of the flight.
        /// </summary>
        public int FlightId { get; set; }


        /// <summary>
        /// Gets or sets the unique identifier of the ticket.
        /// </summary>
        public int TicketId { get; set; }
    }
}