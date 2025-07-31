namespace VitoriaAirlinesWeb.Models.ViewModels.Dashboard
{
    /// <summary>
    /// Represents a view model for displaying information about a top destination.
    /// </summary>
    public class TopDestinationViewModel
    {
        /// <summary>
        /// Gets or sets the name of the country.
        /// </summary>
        public string CountryName { get; set; } = null!;


        /// <summary>
        /// Gets or sets the name of the city.
        /// </summary>
        public string City { get; set; } = null!;


        /// <summary>
        /// Gets or sets the URL to the country's flag image.
        /// </summary>
        public string CountryFlagUrl { get; set; } = null!;


        /// <summary>
        /// Gets or sets the full name of the airport (e.g., "JFK - John F. Kennedy Airport").
        /// </summary>
        public string AirportName { get; set; } = null!;


        /// <summary>
        /// Gets or sets the number of tickets sold to this destination.
        /// </summary>
        public int TicketCount { get; set; }
    }

}