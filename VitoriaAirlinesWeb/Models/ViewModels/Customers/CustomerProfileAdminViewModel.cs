using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace VitoriaAirlinesWeb.Models.ViewModels.Customers
{
    /// <summary>
    /// Represents the view model for editing a customer's profile from an administrator's perspective.
    /// Allows updating country, passport number, and displays basic user info.
    /// </summary>
    public class CustomerProfileAdminViewModel
    {
        /// <summary>
        /// Gets or sets the unique identifier of the customer profile.
        /// </summary>
        public int Id { get; set; }


        /// <summary>
        /// Gets or sets the ID of the customer's nationality/country. Nullable.
        /// </summary>
        [Display(Name = "Nationality")]
        public int? CountryId { get; set; }


        /// <summary>
        /// Gets or sets a collection of countries available for selection in a dropdown list. Nullable.
        /// </summary>
        public IEnumerable<SelectListItem>? Countries { get; set; }


        /// <summary>
        /// Gets or sets the customer's passport number. Nullable and has a max length of 20.
        /// </summary>
        [Display(Name = "Passport Number")]
        [MaxLength(20)]
        public string? PassportNumber { get; set; }


        /// <summary>
        /// Gets or sets the full name of the customer (read-only for display).
        /// </summary>
        public string FullName { get; set; } = string.Empty;


        /// <summary>
        /// Gets or sets the email address of the customer (read-only for display).
        /// </summary>
        public string Email { get; set; } = string.Empty;

    }
}