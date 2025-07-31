namespace VitoriaAirlinesWeb.Models.ViewModels.Dashboard
{
    /// <summary>
    /// Represents the view model for a customer's dashboard, providing an overview of their flight activity and profile.
    /// </summary>
    public class CustomerDashboardViewModel
    {
        /// <summary>
        /// Gets or sets the total number of flights booked by the customer.
        /// </summary>
        public int FlightsBooked { get; set; }


        /// <summary>
        /// Gets or sets the total number of flights completed by the customer.
        /// </summary>
        public int FlightsCompleted { get; set; }


        /// <summary>
        /// Gets or sets the total number of flights canceled by the customer.
        /// </summary>
        public int FlightsCanceled { get; set; }


        /// <summary>
        /// Gets or sets the total amount of money spent by the customer on flights.
        /// </summary>
        public decimal TotalSpent { get; set; }


        /// <summary>
        /// Gets or sets the full name of the customer.
        /// </summary>
        public string FullName { get; set; } = null!;


        /// <summary>
        /// Gets or sets the email address of the customer.
        /// </summary>
        public string Email { get; set; } = null!;


        /// <summary>
        /// Gets or sets the customer's passport number. Nullable.
        /// </summary>
        public string? PassportNumber { get; set; }


        /// <summary>
        /// Gets or sets the name of the customer's country. Nullable.
        /// </summary>
        public string? Country { get; set; }


        /// <summary>
        /// Gets or sets the URL to the customer's country flag image. Nullable.
        /// </summary>
        public string? CountryFlagUrl { get; set; }


        /// <summary>
        /// Gets or sets the URL to the customer's profile picture. Nullable.
        /// </summary>
        public string? ProfilePictureUrl { get; set; }


        /// <summary>
        /// Gets or sets a list of the customer's upcoming flights.
        /// </summary>
        public List<FlightInfoViewModel> UpcomingFlights { get; set; } = null!;


        /// <summary>
        /// Gets or sets a list of the customer's past flights.
        /// </summary>
        public List<FlightInfoViewModel> PastFlights { get; set; } = null!;


        /// <summary>
        /// Gets or sets the most recently completed flight for the customer. Nullable.
        /// </summary>
        public FlightInfoViewModel? LastCompletedFlight { get; set; }


        /// <summary>
        /// Gets or sets the next upcoming flight for the customer. Nullable.
        /// </summary>
        public FlightInfoViewModel? NextUpcomingFlight { get; set; }

    }
}