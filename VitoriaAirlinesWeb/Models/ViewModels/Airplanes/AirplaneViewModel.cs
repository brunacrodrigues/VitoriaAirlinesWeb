using System.ComponentModel.DataAnnotations;
using VitoriaAirlinesWeb.Data.Enums;

namespace VitoriaAirlinesWeb.Models.ViewModels.Airplanes
{
    /// <summary>
    /// Represents the view model for creating or editing an airplane.
    /// Includes validation rules for its properties and an optional image file.
    /// </summary>
    public class AirplaneViewModel
    {
        /// <summary>
        /// Gets or sets the unique identifier for the airplane. This is 0 for new airplanes.
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
        /// Gets or sets the total number of executive class seats. This field is required and must be a non-negative integer.
        /// </summary>
        [Required]
        [Display(Name = "Executive Seats")]
        [Range(0, int.MaxValue)]
        public int TotalExecutiveSeats { get; set; }


        /// <summary>
        /// Gets or sets the total number of economy class seats. This field is required and must be greater than 0.
        /// </summary>
        [Required]
        [Display(Name = "Economy Seats")]
        [Range(1, int.MaxValue, ErrorMessage = "Must be greater than 0")]
        public int TotalEconomySeats { get; set; }

        /// <summary>
        /// Gets or sets the GUID of the airplane's current image.
        /// </summary>
        public Guid ImageId { get; set; }

        /// <summary>
        /// Gets or sets the current operational status of the airplane.
        /// </summary>
        public AirplaneStatus Status { get; set; }


        /// <summary>
        /// Gets or sets the new image file for the airplane, uploaded by the user. This property is nullable.
        /// </summary>
        [Display(Name = "Image")]
        public IFormFile? ImageFile { get; set; }
    }
}
