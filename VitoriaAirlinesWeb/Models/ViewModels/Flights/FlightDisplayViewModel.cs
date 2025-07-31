namespace VitoriaAirlinesWeb.Models.ViewModels.Flights
{
    /// <summary>
    /// Represents a view model for displaying flight information in lists (e.g., flight management).
    /// </summary>
    public class FlightDisplayViewModel
    {
        /// <summary>
        /// Gets or sets the unique identifier of the flight.
        /// </summary>
        public int Id { get; set; }


        /// <summary>
        /// Gets or sets the flight number.
        /// </summary>
        public string FlightNumber { get; set; } = null!;


        /// <summary>
        /// Gets or sets the model name of the airplane assigned to the flight.
        /// </summary>
        public string AirplaneModel { get; set; } = null!;


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
        /// Gets or sets the formatted departure date and time.
        /// </summary>
        public string Departure { get; set; } = null!;


        /// <summary>
        /// Gets or sets the formatted arrival date and time.
        /// </summary>
        public string Arrival { get; set; } = null!;


        /// <summary>
        /// Gets or sets the formatted duration of the flight.
        /// </summary>
        public string Duration { get; set; } = null!;


        /// <summary>
        /// Gets or sets the executive class price for the flight.
        /// </summary>
        public decimal ExecutiveClassPrice { get; set; }


        /// <summary>
        /// Gets or sets the economy class price for the flight.
        /// </summary>
        public decimal EconomyClassPrice { get; set; }


        /// <summary>
        /// Gets or sets the status of the flight (e.g., "Scheduled", "Departed").
        /// </summary>
        public string Status { get; set; } = null!;
    }
}