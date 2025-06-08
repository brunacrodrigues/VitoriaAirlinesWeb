using System.ComponentModel.DataAnnotations;

namespace VitoriaAirlinesWeb.Models
{
    public class RegisterNewUserViewModel
    {
        [Required]
        [Display(Name = "First Name")]
        [MaxLength(100, ErrorMessage = "This field must have {0} characters or less.")]
        public string FirstName { get; set; } = null!;


        [Required]
        [Display(Name = "Last Name")]
        [MaxLength(100, ErrorMessage = "This field must have {0} characters or less.")]
        public string LastName { get; set; } = null!;


        [Required]
        [DataType(DataType.EmailAddress)]
        public string Username { get; set; } = null!;


        [Required]
        [DataType(DataType.Password)]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
        public string Password { get; set; } = null!;


        [Required]
        [Display(Name = "Confirm Password")]
        [DataType(DataType.Password)]
        [Compare("Password")]
        
        public string ConfirmPassword { get; set; } = null!;


    }
}
