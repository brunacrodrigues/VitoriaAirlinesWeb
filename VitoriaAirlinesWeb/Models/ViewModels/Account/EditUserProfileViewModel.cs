using System.ComponentModel.DataAnnotations;

namespace VitoriaAirlinesWeb.Models.ViewModels.Account
{
    public class EditUserProfileViewModel
    {
        [Required]
        [Display(Name = "First Name")]
        [MaxLength(100, ErrorMessage = "This field must have {0} characters or less.")]
        public string FirstName { get; set; } = null!;


        [Required]
        [Display(Name = "Last Name")]
        [MaxLength(100, ErrorMessage = "This field must have {0} characters or less.")]
        public string LastName { get; set; } = null!;


        public string? CurrentProfileImagePath { get; set; }


        [Display(Name = "Profile Image")]
        public IFormFile? ImageFile { get; set; }

    }
}
