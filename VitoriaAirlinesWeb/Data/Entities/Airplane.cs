using System.ComponentModel.DataAnnotations;
using VitoriaAirlinesWeb.Data.Enums;

namespace VitoriaAirlinesWeb.Data.Entities
{
    /// <summary>
    /// Represents an airplane entity in the Vitoria Airlines system.
    /// Includes details about its model, seat configuration, status, and associated image.
    /// </summary>
    public class Airplane : IEntity
    {
        /// <summary>
        /// Gets or sets the unique identifier for the airplane.
        /// </summary>
        public int Id { get; set; }


        /// <summary>
        /// Gets or sets the model name of the airplane. This field is required and has a maximum length of 50 characters.
        /// </summary>
        [Required]
        [MaxLength(50)]
        [Display(Name = "Model")]
        public string Model { get; set; } = null!;


        /// <summary>
        /// Gets or sets the total number of executive class seats available on the airplane.
        /// The value must be a non-negative integer.
        /// </summary>
        [Display(Name = "Executive Seats")]
        [Range(0, int.MaxValue)]
        public int TotalExecutiveSeats { get; set; }


        /// <summary>
        /// Gets or sets the total number of economy class seats available on the airplane.
        /// The value must be a non-negative integer.
        /// </summary>
        [Display(Name = "Economy Seats")]
        [Range(0, int.MaxValue)]
        public int TotalEconomySeats { get; set; }


        /// <summary>
        /// Gets or sets the unique identifier (GUID) for the airplane's image stored in blob storage. This field is required.
        /// </summary>
        [Required]
        public Guid ImageId { get; set; }


        /// <summary>
        /// Gets or sets the collection of seats associated with this airplane.
        /// This represents the physical seating layout of the airplane.
        /// </summary>
        public ICollection<Seat> Seats { get; set; }


        /// <summary>
        /// Gets or sets the current operational status of the airplane. This field is required.
        /// </summary>
        [Required]
        public AirplaneStatus Status { get; set; }


        /// <summary>
        /// Gets the full URL path to the airplane's image.
        /// If ImageId is empty, it returns a URL to a default "no image" placeholder.
        /// </summary>
        public string ImageFullPath => ImageId == Guid.Empty
        ? $"https://brunablob.blob.core.windows.net/images/noimage.png"
        : $"https://brunablob.blob.core.windows.net/images/{ImageId}";


    }
}
