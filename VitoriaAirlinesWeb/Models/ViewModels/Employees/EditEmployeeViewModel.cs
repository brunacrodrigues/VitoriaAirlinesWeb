using System.ComponentModel.DataAnnotations;

namespace VitoriaAirlinesWeb.Models.ViewModels.Employees
{
    public class EditEmployeeViewModel
    {
        public string Email { get; set; } = null!;

        [Required]
        [Display(Name = "First Name")]
        [MaxLength(100, ErrorMessage = "This field must have {1} characters or less.")]
        public string FirstName { get; set; } = null!;


        [Required]
        [Display(Name = "Last Name")]
        [MaxLength(100, ErrorMessage = "This field must have {1} characters or less.")]
        public string LastName { get; set; } = null!;
    }
}
