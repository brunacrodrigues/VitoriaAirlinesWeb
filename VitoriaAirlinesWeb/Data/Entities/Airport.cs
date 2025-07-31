using System.ComponentModel.DataAnnotations;

namespace VitoriaAirlinesWeb.Data.Entities
{
    /// <summary>
    /// Represents an airport entity in the Vitoria Airlines system.
    /// Includes details about its IATA code, name, city, and associated country.
    /// </summary>
    public class Airport : IEntity
    {
        /// <summary>
        /// Gets or sets the unique identifier for the airport.
        /// </summary>
        public int Id { get; set; }


        /// <summary>
        /// Gets or sets the 3-letter IATA code of the airport. This field is required and has a fixed length of 3 characters.
        /// </summary>
        [Required]
        [StringLength(3, MinimumLength = 3)]
        public string IATA { get; set; } = null!;


        /// <summary>
        /// Gets or sets the full name of the airport. This field is required and has a maximum length of 100 characters.
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = null!;


        /// <summary>
        /// Gets or sets the city where the airport is located. This field is required and has a maximum length of 100 characters.
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string City { get; set; } = null!;


        /// <summary>
        /// Gets or sets the foreign key to the associated country. This field is required.
        /// </summary>
        [Required]
        public int CountryId { get; set; }


        /// <summary>
        /// Gets or sets the navigation property to the associated Country entity.
        /// This represents the country where the airport is located.
        /// </summary>
        public Country Country { get; set; } = null!;



        /// <summary>
        /// Gets the full name of the airport, combining its IATA code and name (e.g., "JFK - John F. Kennedy Airport").
        /// </summary>
        public string FullName => $"{IATA} - {Name}";


    }
}