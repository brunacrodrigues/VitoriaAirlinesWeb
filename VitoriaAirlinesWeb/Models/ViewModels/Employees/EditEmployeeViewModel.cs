using System.ComponentModel.DataAnnotations;

namespace VitoriaAirlinesWeb.Models.ViewModels.Employees
{
    /// <summary>
    /// Represents the view model for editing an existing employee's profile.
    /// </summary>
    public class EditEmployeeViewModel
    {
        /// <summary>
        /// Gets or sets the email address of the employee.
        /// </summary>
        public string Email { get; set; } = null!;


        /// <summary>
        /// Gets or sets the first name of the employee. This field is required and has a maximum length of 100 characters.
        /// </summary>
        [Required]
        [Display(Name = "First Name")]
        [MaxLength(100, ErrorMessage = "This field must have {1} characters or less.")]
        public string FirstName { get; set; } = null!;


        /// <summary>
        /// Gets or sets the last name of the employee. This field is required and has a maximum length of 100 characters.
        /// </summary>
        [Required]
        [Display(Name = "Last Name")]
        [MaxLength(100, ErrorMessage = "This field must have {1} characters or less.")]
        public string LastName { get; set; } = null!;
    }
}