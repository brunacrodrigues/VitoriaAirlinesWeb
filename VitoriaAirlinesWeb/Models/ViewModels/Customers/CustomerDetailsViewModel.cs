using VitoriaAirlinesWeb.Data.Entities;

namespace VitoriaAirlinesWeb.Models.ViewModels.Customers
{
    /// <summary>
    /// Represents the view model for displaying detailed customer information to administrators.
    /// Includes customer profile, total flights, and details of last/next flights.
    /// </summary>
    public class CustomerDetailsViewModel
    {
        /// <summary>
        /// Gets or sets the CustomerProfile entity associated with the details.
        /// </summary>
        public CustomerProfile Customer { get; set; } = null!;


        /// <summary>
        /// Gets or sets the total number of flights booked by the customer.
        /// </summary>
        public int TotalFlights { get; set; }


        /// <summary>
        /// Gets or sets the last completed flight for the customer. Nullable.
        /// </summary>
        public Flight? LastFlight { get; set; }


        /// <summary>
        /// Gets or sets the next upcoming flight for the customer. Nullable.
        /// </summary>
        public Flight? NextFlight { get; set; }


        /// <summary>
        /// Gets or sets the primary role of the customer (e.g., "Customer", "Deactivated").
        /// </summary>
        public string Role { get; set; } = null!; // Assumes initialization

    }
}