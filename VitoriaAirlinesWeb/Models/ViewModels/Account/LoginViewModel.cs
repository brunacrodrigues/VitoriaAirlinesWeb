using System.ComponentModel.DataAnnotations;

namespace VitoriaAirlinesWeb.Models.ViewModels.Account
{
    /// <summary>
    /// Represents the view model for user login.
    /// </summary>
    public class LoginViewModel
    {
        /// <summary>
        /// Gets or sets the username (email address) for login. This field is required and should be a valid email format.
        /// </summary>
        [Required]
        [DataType(DataType.EmailAddress)]
        public string Username { get; set; } = null!;

        /// <summary>
        /// Gets or sets the password for login. This field is required and must be at least 8 characters long.
        /// </summary>
        [Required]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;


        /// <summary>
        /// Gets or sets a value indicating whether the user wants to be remembered (for persistent login).
        /// </summary>
        public bool RememberMe { get; set; }
    }
}