using System.ComponentModel.DataAnnotations;

namespace VitoriaAirlinesWeb.Data.Entities
{
    /// <summary>
    /// Represents a customer's profile information, extending the base User entity.
    /// Includes details such as country of residence and passport number.
    /// </summary>
    public class CustomerProfile : IEntity
    {
        /// <summary>
        /// Gets or sets the unique identifier for the customer profile.
        /// </summary>
        public int Id { get; set; }


        /// <summary>
        /// Gets or sets the foreign key to the associated country of residence.
        /// This property is nullable, indicating that a country may not be specified.
        /// </summary>
        public int? CountryId { get; set; }


        /// <summary>
        /// Gets or sets the navigation property to the associated Country entity.
        /// This property is nullable.
        /// </summary>
        public Country? Country { get; set; }


        /// <summary>
        /// Gets or sets the passport number of the customer. This field has a maximum length of 20 characters.
        /// This property is nullable.
        /// </summary>
        [MaxLength(20)]
        public string? PassportNumber { get; set; }


        /// <summary>
        /// Gets or sets the foreign key (string GUID) to the associated User entity. This field is required.
        /// </summary>
        [Required]
        public string UserId { get; set; } = null!;


        /// <summary>
        /// Gets or sets the navigation property to the associated User entity. This field is required.
        /// </summary>
        public User User { get; set; } = null!;



    }
}
