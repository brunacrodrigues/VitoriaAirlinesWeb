using System.ComponentModel.DataAnnotations;

namespace VitoriaAirlinesWeb.Models.ViewModels.Employees
{
    /// <summary>
    /// Represents the view model for registering a new employee.
    /// </summary>
    public class RegisterEmployeeViewModel
    {
        /// <summary>
        /// Gets or sets the first name of the new employee. This field is required and has a maximum length of 100 characters.
        /// </summary>
        [Required]
        [Display(Name = "First Name")]
        [MaxLength(100, ErrorMessage = "This field must have {1} characters or less.")]
        public string FirstName { get; set; } = null!;


        /// <summary>
        /// Gets or sets the last name of the new employee. This field is required and has a maximum length of 100 characters.
        /// </summary>
        [Required]
        [Display(Name = "Last Name")]
        [MaxLength(100, ErrorMessage = "This field must have {1} characters or less.")]
        public string LastName { get; set; } = null!;


        /// <summary>
        /// Gets or sets the username (email address) for the new employee. This field is required and should be a valid email format.
        /// </summary>
        [Required]
        [DataType(DataType.EmailAddress)]
        public string Username { get; set; } = null!;
    }
}