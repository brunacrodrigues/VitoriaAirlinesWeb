using System.ComponentModel.DataAnnotations;

namespace VitoriaAirlinesWeb.Models.Employees
{
    public class RegisterEmployeeViewModel
    {
        [Required]
        [Display(Name = "First Name")]
        [MaxLength(100, ErrorMessage = "This field must have {1} characters or less.")]
        public string FirstName { get; set; } = null!;


        [Required]
        [Display(Name = "Last Name")]
        [MaxLength(100, ErrorMessage = "This field must have {1} characters or less.")]
        public string LastName { get; set; } = null!;


        [Required]
        [DataType(DataType.EmailAddress)]
        public string Username { get; set; } = null!;
    }
}
