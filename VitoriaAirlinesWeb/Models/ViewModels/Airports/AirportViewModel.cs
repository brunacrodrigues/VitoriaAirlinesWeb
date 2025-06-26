using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace VitoriaAirlinesWeb.Models.Airports
{
    public class AirportViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "IATA code is required.")]
        [StringLength(3, MinimumLength = 3)]
        [RegularExpression("^[A-Z]{3}$", ErrorMessage = "IATA code must be exactly 3 uppercase letters.")]
        public string IATA { get; set; } = null!;


        [Required(ErrorMessage = "Airport name is required.")]
        [MaxLength(100, ErrorMessage = "Name can have a maximum of 100 characters.")]
        public string Name { get; set; } = null!;


        [Required(ErrorMessage = "City is required.")]
        [MaxLength(100)]
        public string City { get; set; } = null!;


        [Display(Name = "Country")]
        [Required(ErrorMessage = "Please select a country.")]
        public int CountryId { get; set; }


        public IEnumerable<SelectListItem>? Countries { get; set; }
    }
}
