using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace VitoriaAirlinesWeb.Models.Airports
{
    /// <summary>
    /// Represents the view model for creating or editing an airport.
    /// Includes validation rules for its properties and a list of countries for selection.
    /// </summary>
    public class AirportViewModel
    {
        /// <summary>
        /// Gets or sets the unique identifier for the airport. This is 0 for new airports.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the 3-letter IATA code of the airport. It is required, must be exactly 3 uppercase letters.
        /// </summary>
        [Required(ErrorMessage = "IATA code is required.")]
        [StringLength(3, MinimumLength = 3)]
        [RegularExpression("^[A-Z]{3}$", ErrorMessage = "IATA code must be exactly 3 uppercase letters.")]
        public string IATA { get; set; } = null!;


        /// <summary>
        /// Gets or sets the full name of the airport. It is required and has a maximum length of 100 characters.
        /// </summary>
        [Required(ErrorMessage = "Airport name is required.")]
        [MaxLength(100, ErrorMessage = "Name can have a maximum of 100 characters.")]
        public string Name { get; set; } = null!;


        /// <summary>
        /// Gets or sets the city where the airport is located. It is required and has a maximum length of 100 characters.
        /// </summary>
        [Required(ErrorMessage = "City is required.")]
        [MaxLength(100)]
        public string City { get; set; } = null!;


        /// <summary>
        /// Gets or sets the selected country ID for the airport. It is required.
        /// </summary>
        [Display(Name = "Country")]
        [Required(ErrorMessage = "Please select a country.")]
        public int? CountryId { get; set; }


        /// <summary>
        /// Gets or sets a collection of countries available for selection in a dropdown list. This property is nullable.
        /// </summary>
        public IEnumerable<SelectListItem>? Countries { get; set; }
    }
}
