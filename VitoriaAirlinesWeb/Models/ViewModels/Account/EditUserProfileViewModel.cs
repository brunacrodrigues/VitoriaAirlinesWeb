using System.ComponentModel.DataAnnotations;

namespace VitoriaAirlinesWeb.Models.ViewModels.Account
{
    /// <summary>
    /// Represents the view model for editing a user's profile information.
    /// </summary>
    public class EditUserProfileViewModel
    {
        /// <summary>
        /// Gets or sets the user's first name. This field is required and has a maximum length of 100 characters.
        /// </summary>
        [Required]
        [Display(Name = "First Name")]
        [MaxLength(100, ErrorMessage = "This field must have {0} characters or less.")]
        public string FirstName { get; set; } = null!;


        /// <summary>
        /// Gets or sets the user's last name. This field is required and has a maximum length of 100 characters.
        /// </summary>
        [Required]
        [Display(Name = "Last Name")]
        [MaxLength(100, ErrorMessage = "This field must have {0} characters or less.")]
        public string LastName { get; set; } = null!;


        /// <summary>
        /// Gets or sets the current path or URL to the user's profile image. This property is nullable.
        /// </summary>
        public string? CurrentProfileImagePath { get; set; }


        /// <summary>
        /// Gets or sets the new profile image file uploaded by the user. This property is nullable.
        /// </summary>
        [Display(Name = "Profile Image")]
        public IFormFile? ImageFile { get; set; }

    }
}