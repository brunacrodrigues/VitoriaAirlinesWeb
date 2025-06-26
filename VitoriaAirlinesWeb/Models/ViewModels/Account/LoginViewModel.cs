using System.ComponentModel.DataAnnotations;

namespace VitoriaAirlinesWeb.Models.ViewModels.Account
{
    public class LoginViewModel
    {
        [Required]
        [DataType(DataType.EmailAddress)]
        public string Username { get; set; } = null!;

        [Required]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]

        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;


        public bool RememberMe { get; set; }
    }
}
