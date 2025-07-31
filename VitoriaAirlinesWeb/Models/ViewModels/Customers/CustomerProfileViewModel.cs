using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace VitoriaAirlinesWeb.Models.ViewModels.Customers
{
    /// <summary>
    /// Represents the view model for a customer editing their own profile.
    /// Allows updating first name, last name, profile image, nationality, and passport number.
    /// </summary>
    public class CustomerProfileViewModel
    {

        /// <summary>
        /// Gets or sets the user's first name. Required and max length 100.
        /// </summary>
        [Required]
        [Display(Name = "First Name")]
        [MaxLength(100)]
        public string FirstName { get; set; } = null!;


        /// <summary>
        /// Gets or sets the user's last name. Required and max length 100.
        /// </summary>
        [Required]
        [Display(Name = "Last Name")]
        [MaxLength(100)]
        public string LastName { get; set; } = null!;


        /// <summary>
        /// Gets or sets the current path/URL to the user's profile image. Nullable.
        /// </summary>
        public string? CurrentProfileImagePath { get; set; }


        /// <summary>
        /// Gets or sets the new profile image file uploaded by the user. Nullable.
        /// </summary>
        [Display(Name = "Profile Image")]
        public IFormFile? ImageFile { get; set; }


        /// <summary>
        /// Gets or sets the ID of the user's nationality/country. Nullable.
        /// </summary>
        [Display(Name = "Nationality")]
        public int? CountryId { get; set; }


        /// <summary>
        /// Gets or sets a collection of countries available for selection in a dropdown list. Nullable.
        /// </summary>
        public IEnumerable<SelectListItem>? Countries { get; set; }


        /// <summary>
        /// Gets or sets the user's passport number. Nullable and max length 20.
        /// </summary>
        [Display(Name = "Passport Number")]
        [MaxLength(20)]
        public string? PassportNumber { get; set; }
    }
}