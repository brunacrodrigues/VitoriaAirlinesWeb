using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace VitoriaAirlinesWeb.Models.ViewModels.Customers
{
    public class CustomerProfileViewModel
    {
        [Display(Name = "Nationality")]
        public int? CountryId { get; set; }


        public IEnumerable<SelectListItem>? Countries { get; set; }


        [Display(Name = "Passport Number")]
        [MaxLength(20)]
        public string? PassportNumber { get; set; }
    }
}
