using System.ComponentModel.DataAnnotations;

namespace VitoriaAirlinesWeb.Models.ViewModels.Account
{
    /// <summary>
    /// Represents the view model for new user registration.
    /// </summary>
    public class RegisterNewUserViewModel
    {
        /// <summary>
        /// Gets or sets the first name of the new user. This field is required and has a maximum length of 100 characters.
        /// </summary>
        [Required]
        [Display(Name = "First Name")]
        [MaxLength(100, ErrorMessage = "This field must have {0} characters or less.")]
        public string FirstName { get; set; } = null!;


        /// <summary>
        /// Gets or sets the last name of the new user. This field is required and has a maximum length of 100 characters.
        /// </summary>
        [Required]
        [Display(Name = "Last Name")]
        [MaxLength(100, ErrorMessage = "This field must have {0} characters or less.")]
        public string LastName { get; set; } = null!;


        /// <summary>
        /// Gets or sets the username (email address) for the new account. This field is required and should be a valid email format.
        /// </summary>
        [Required]
        [DataType(DataType.EmailAddress)]
        public string Username { get; set; } = null!;


        /// <summary>
        /// Gets or sets the password for the new account. This field is required and must be at least 8 characters long.
        /// </summary>
        [Required]
        [DataType(DataType.Password)]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
        public string Password { get; set; } = null!;


        /// <summary>
        /// Gets or sets the confirmation of the new password. This field is required and must match the Password.
        /// </summary>
        [Required]
        [Display(Name = "Confirm Password")]
        [DataType(DataType.Password)]
        [Compare("Password")]

        public string ConfirmPassword { get; set; } = null!;


    }
}