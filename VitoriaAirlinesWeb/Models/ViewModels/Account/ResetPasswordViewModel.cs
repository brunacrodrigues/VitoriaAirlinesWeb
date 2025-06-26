using System.ComponentModel.DataAnnotations;

namespace VitoriaAirlinesWeb.Models.ViewModels.Account
{
    public class ResetPasswordViewModel
    {
        [Required]
        public string Username { get; set; } = null!;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;


        [Required]
        [DataType(DataType.Password)]
        [Compare("Password")]
        public string ConfirmPassword { get; set; } = null!;


        [Required]
        public string Token { get; set; } = null!;
    }
}
