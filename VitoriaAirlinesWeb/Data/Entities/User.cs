using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace VitoriaAirlinesWeb.Data.Entities
{
    /// <summary>
    /// Represents a user entity in the Vitoria Airlines system, extending the base IdentityUser.
    /// Includes additional properties for first name, last name, profile image, and navigation to related entities.
    /// </summary>
    public class User : IdentityUser
    {
        /// <summary>
        /// Gets or sets the first name of the user. This field has a maximum length of 100 characters.
        /// </summary>
        [MaxLength(100, ErrorMessage = "The field {0} only can contain {1} characters long.")]
        public string FirstName { get; set; } = null!;


        /// <summary>
        /// Gets or sets the last name of the user. This field has a maximum length of 100 characters.
        /// </summary>
        [MaxLength(100, ErrorMessage = "The field {0} only can contain {1} characters long.")]
        public string LastName { get; set; } = null!;


        /// <summary>
        /// Gets or sets the unique identifier (GUID) for the user's profile image stored in blob storage. This property is nullable.
        /// </summary>
        public Guid? ProfileImageId { get; set; }


        /// <summary>
        /// Gets the full URL path to the user's profile image.
        /// If ProfileImageId is null, it returns a URL to a default "profile picture" placeholder.
        /// </summary>
        public string ImageFullPath => ProfileImageId == null
            ? "https://brunablob.blob.core.windows.net/images/profilepic.png"
            : $"https://brunablob.blob.core.windows.net/images/{ProfileImageId}";


        /// <summary>
        /// Gets the full name of the user, concatenating their first and last names.
        /// </summary>
        public string FullName => $"{FirstName} {LastName}";


        /// <summary>
        /// Gets or sets the navigation property to the associated CustomerProfile entity. This property is nullable.
        /// A user may or may not have an associated customer profile (e.g., admin or employee users might not).
        /// </summary>
        public CustomerProfile? CustomerProfile { get; set; }


        /// <summary>
        /// Gets or sets the collection of tickets associated with this user.
        /// </summary>
        public ICollection<Ticket> Tickets { get; set; }



    }
}
