using System.ComponentModel.DataAnnotations;

namespace VitoriaAirlinesWeb.Data.Entities
{
    /// <summary>
    /// Represents a country entity in the Vitoria Airlines system.
    /// Includes details about its name, country code, and a derived flag image URL.
    /// </summary>
    public class Country : IEntity
    {
        /// <summary>
        /// Gets or sets the unique identifier for the country.
        /// </summary>
        public int Id { get; set; }


        /// <summary>
        /// Gets or sets the name of the country. This field is required and has a maximum length of 100 characters.
        /// </summary>
        [MaxLength(100)]
        [Required]
        public string Name { get; set; } = null!;


        /// <summary>
        /// Gets or sets the two-letter ISO country code (e.g., "US", "BR"). This field is required and has a fixed length of 2 characters.
        /// </summary>
        [MaxLength(2)]
        [Required]
        public string CountryCode { get; set; } = null!;


        /// <summary>
        /// Gets the URL for the country's flag image, derived from its country code.
        /// Uses 'flagcdn.com' as the image source.
        /// </summary>
        public string FlagImageUrl => $"https://flagcdn.com/w40/{CountryCode.ToLower()}.png";
    }
}
