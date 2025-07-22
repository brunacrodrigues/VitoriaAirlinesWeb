using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace VitoriaAirlinesWeb.Models.ViewModels.Customers
{
    public class CustomerProfileViewModel
    {

        [Required]
        [Display(Name = "First Name")]
        [MaxLength(100)]
        public string FirstName { get; set; } = null!;

        [Required]
        [Display(Name = "Last Name")]
        [MaxLength(100)]
        public string LastName { get; set; } = null!;

        public string? CurrentProfileImagePath { get; set; }

        [Display(Name = "Profile Image")]
        public IFormFile? ImageFile { get; set; }


        [Display(Name = "Nationality")]
        public int? CountryId { get; set; }

        public IEnumerable<SelectListItem>? Countries { get; set; }

        [Display(Name = "Passport Number")]
        [MaxLength(20)]
        public string? PassportNumber { get; set; }
    }
}
