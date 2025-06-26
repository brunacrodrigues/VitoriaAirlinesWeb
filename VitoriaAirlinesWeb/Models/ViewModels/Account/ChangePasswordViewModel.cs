using System.ComponentModel.DataAnnotations;

namespace VitoriaAirlinesWeb.Models.ViewModels.Account
{
    public class ChangePasswordViewModel
    {
        [Required]
        [Display(Name = "Current Password")]
        [DataType(DataType.Password)]
        public string OldPassword { get; set; } = null!;

        [Required]
        [Display(Name = "New Password")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; } = null!;

        [Required]
        [Compare("NewPassword")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = null!;
    }
}
