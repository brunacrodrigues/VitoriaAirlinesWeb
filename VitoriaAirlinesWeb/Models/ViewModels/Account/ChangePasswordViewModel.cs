using System.ComponentModel.DataAnnotations;

namespace VitoriaAirlinesWeb.Models.ViewModels.Account
{
    /// <summary>
    /// Represents the view model for changing a user's password.
    /// </summary>
    public class ChangePasswordViewModel
    {
        /// <summary>
        /// Gets or sets the user's current password. This field is required.
        /// </summary>
        [Required]
        [Display(Name = "Current Password")]
        [DataType(DataType.Password)]
        public string OldPassword { get; set; } = null!;


        /// <summary>
        /// Gets or sets the user's new password. This field is required.
        /// </summary>
        [Required]
        [Display(Name = "New Password")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; } = null!;


        /// <summary>
        /// Gets or sets the confirmation of the new password. This field is required and must match NewPassword.
        /// </summary>
        [Required]
        [Compare("NewPassword")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = null!;

    }
}
