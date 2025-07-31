namespace VitoriaAirlinesWeb.Models.ViewModels.Customers
{
    /// <summary>
    /// Represents a simplified view model for displaying customer information in a list.
    /// </summary>
    public class CustomerViewModel
    {
        /// <summary>
        /// Gets or sets the unique identifier of the customer profile.
        /// </summary>
        public int Id { get; set; }


        /// <summary>
        /// Gets or sets the full name of the customer.
        /// </summary>
        public string FullName { get; set; } = null!;


        /// <summary>
        /// Gets or sets the email address of the customer.
        /// </summary>
        public string Email { get; set; } = null!;


        /// <summary>
        /// Gets or sets the name of the customer's country. Nullable.
        /// </summary>
        public string? CountryName { get; set; }


        /// <summary>
        /// Gets or sets the URL to the customer's country flag image. Nullable.
        /// </summary>
        public string? CountryFlagUrl { get; set; }


        /// <summary>
        /// Gets or sets the customer's passport number. Nullable.
        /// </summary>
        public string? PassportNumber { get; set; }


        /// <summary>
        /// Gets or sets the primary role of the customer (e.g., "Customer", "Deactivated"). Nullable.
        /// </summary>
        public string? Role { get; set; }

    }
}