namespace VitoriaAirlinesWeb.Models.ViewModels.Dashboard
{
    /// <summary>
    /// Represents a simplified view model for displaying flight information on a dashboard.
    /// </summary>
    public class FlightDashboardViewModel
    {
        /// <summary>
        /// Gets or sets the unique identifier of the flight.
        /// </summary>
        public int Id { get; set; }


        /// <summary>
        /// Gets or sets the flight number.
        /// </summary>
        public string FlightNumber { get; set; } = null!; // Assumes initialization


        /// <summary>
        /// Gets or sets the model name of the airplane assigned to the flight.
        /// </summary>
        public string AirplaneModel { get; set; } = null!; // Assumes initialization


        /// <summary>
        /// Gets or sets the full name of the origin airport.
        /// </summary>
        public string OriginAirportFullName { get; set; } = null!; // Assumes initialization


        /// <summary>
        /// Gets or sets the URL to the origin country's flag image.
        /// </summary>
        public string OriginCountryFlagUrl { get; set; } = null!; // Assumes initialization


        /// <summary>
        /// Gets or sets the full name of the destination airport.
        /// </summary>
        public string DestinationAirportFullName { get; set; } = null!; // Assumes initialization


        /// <summary>
        /// Gets or sets the URL to the destination country's flag image.
        /// </summary>
        public string DestinationCountryFlagUrl { get; set; } = null!; // Assumes initialization


        /// <summary>
        /// Gets or sets the formatted departure date and time for display.
        /// </summary>
        public string DepartureFormatted { get; set; } = null!; // Assumes initialization


        /// <summary>
        /// Gets or sets the departure date and time in ISO 8601 format for precise sorting/parsing.
        /// </summary>
        public string DepartureIso { get; set; } = null!; // Assumes initialization

    }
}