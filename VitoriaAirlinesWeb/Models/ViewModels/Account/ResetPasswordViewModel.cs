using System.ComponentModel.DataAnnotations;

namespace VitoriaAirlinesWeb.Models.ViewModels.Account
{
    /// <summary>
    /// Represents the view model for resetting a user's password after a reset token has been issued.
    /// </summary>
    public class ResetPasswordViewModel
    {
        /// <summary>
        /// Gets or sets the username (email address) of the account whose password is being reset. This field is required.
        /// </summary>
        [Required]
        public string Username { get; set; } = null!;

        /// <summary>
        /// Gets or sets the new password. This field is required.
        /// </summary>
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;


        /// <summary>
        /// Gets or sets the confirmation of the new password. This field is required and must match the Password.
        /// </summary>
        [Required]
        [DataType(DataType.Password)]
        [Compare("Password")]
        public string ConfirmPassword { get; set; } = null!;


        /// <summary>
        /// Gets or sets the password reset token. This field is required.
        /// </summary>
        [Required]
        public string Token { get; set; } = null!;
    }
}