using System.ComponentModel.DataAnnotations;

namespace VitoriaAirlinesWeb.Models.ViewModels.Account
{
    /// <summary>
    /// Represents the view model for recovering a user's password (by requesting a reset email).
    /// </summary>
    public class RecoverPasswordViewModel
    {
        /// <summary>
        /// Gets or sets the email address associated with the account to recover. This field is required and must be a valid email format.
        /// </summary>
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;
    }
}