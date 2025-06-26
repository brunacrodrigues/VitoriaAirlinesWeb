using System.ComponentModel.DataAnnotations;

namespace VitoriaAirlinesWeb.Models.ViewModels.Account
{
    public class RecoverPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;
    }
}
