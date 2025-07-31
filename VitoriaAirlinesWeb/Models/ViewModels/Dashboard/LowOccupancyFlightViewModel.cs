namespace VitoriaAirlinesWeb.Models.ViewModels.Dashboard
{
    /// <summary>
    /// Represents a view model for displaying upcoming flights with low occupancy rates.
    /// </summary>
    public class LowOccupancyFlightViewModel
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
        /// Gets or sets the full name of the origin airport.
        /// </summary>
        public string OriginAirportFullName { get; set; } = null!;


        /// <summary>
        /// Gets or sets the URL to the origin country's flag image.
        /// </summary>
        public string OriginCountryFlagUrl { get; set; } = null!;


        /// <summary>
        /// Gets or sets the full name of the destination airport.
        /// </summary>
        public string DestinationAirportFullName { get; set; } = null!;


        /// <summary>
        /// Gets or sets the URL to the destination country's flag image.
        /// </summary>
        public string DestinationCountryFlagUrl { get; set; } = null!;


        /// <summary>
        /// Gets or sets the formatted departure date and time for display.
        /// </summary>
        public string DepartureFormatted { get; set; } = null!;


        /// <summary>
        /// Gets or sets the calculated occupancy rate for the flight (as a percentage).
        /// </summary>
        public double OccupancyRate { get; set; }
    }

}