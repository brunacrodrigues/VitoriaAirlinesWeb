using System.ComponentModel.DataAnnotations;
using VitoriaAirlinesWeb.Data.Enums;
using VitoriaAirlinesWeb.Models.Airports;
using VitoriaAirlinesWeb.Models.ViewModels.Airplanes;

namespace VitoriaAirlinesWeb.Models.ViewModels.Flights
{
    /// <summary>
    /// Represents the view model for creating or editing a flight.
    /// Includes validation rules for its properties and collections for dropdown selections.
    /// </summary>
    public class FlightViewModel
    {
        /// <summary>
        /// Gets or sets the unique identifier for the flight. This is 0 for new flights.
        /// </summary>
        public int Id { get; set; }


        /// <summary>
        /// Gets or sets the flight number. Nullable for creation as it might be generated.
        /// </summary>
        [Display(Name = "Flight Number")]
        public string? FlightNumber { get; set; }


        /// <summary>
        /// Gets or sets the ID of the origin airport. This field is required.
        /// </summary>
        [Required(ErrorMessage = "You must select the origin airport.")]
        public int? OriginAirportId { get; set; }


        /// <summary>
        /// Gets or sets the ID of the destination airport. This field is required.
        /// </summary>
        [Required(ErrorMessage = "You must select the destination airport.")]
        public int? DestinationAirportId { get; set; }


        /// <summary>
        /// Gets or sets the ID of the assigned airplane. This field is required and must be greater than 0.
        /// </summary>
        [Display(Name = "Airplane Model")]
        [Range(1, int.MaxValue, ErrorMessage = "You must select an airplane model.")]
        public int AirplaneId { get; set; }


        /// <summary>
        /// Gets or sets the price for economy class tickets. This field is required, must be greater than 0, and formatted as currency.
        /// </summary>
        [Required(ErrorMessage = "Economy Class price is required.")]
        [Display(Name = "Economy Class Price")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Economy price must be greater than 0.")]
        [DisplayFormat(DataFormatString = "{0:C2}", ApplyFormatInEditMode = false)]
        public decimal EconomyClassPrice { get; set; }


        /// <summary>
        /// Gets or sets the price for executive class tickets. This field is required, must be greater than 0, and formatted as currency.
        /// </summary>
        [Required(ErrorMessage = "Executive Class price is required.")]
        [Display(Name = "Executive Class Price")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Executive price must be greater than 0.")]
        [DisplayFormat(DataFormatString = "{0:C2}", ApplyFormatInEditMode = false)]
        public decimal ExecutiveClassPrice { get; set; }


        /// <summary>
        /// Gets or sets the departure date of the flight. This field is required and expected to be a date.
        /// </summary>
        [Required(ErrorMessage = "Departure date is required.")]
        [Display(Name = "Departure Date")]
        [DataType(DataType.Date)]
        public DateOnly? DepartureDate { get; set; }


        /// <summary>
        /// Gets or sets the departure time of the flight. This field is required and expected to be a time.
        /// </summary>
        [Required(ErrorMessage = "Departure time is required.")]
        [Display(Name = "Departure Time")]
        [DataType(DataType.Time)]
        public TimeOnly? DepartureTime { get; set; }


        /// <summary>
        /// Gets or sets the duration of the flight. This field is required and formatted as HH:mm.
        /// </summary>
        [Required(ErrorMessage = "Flight duration is required.")]
        [Display(Name = "Flight Duration (HH:mm)")]
        public TimeSpan? Duration { get; set; }

        /// <summary>
        /// Gets or sets the current status of the flight.
        /// </summary>
        public FlightStatus Status { get; set; }


        /// <summary>
        /// Gets or sets a collection of available destination airports for selection. Nullable.
        /// </summary>
        public IEnumerable<AirportDropdownViewModel>? DestinationAirports { get; set; }


        /// <summary>
        /// Gets or sets a collection of available origin airports for selection. Nullable.
        /// </summary>
        public IEnumerable<AirportDropdownViewModel>? OriginAirports { get; set; }


        /// <summary>
        /// Gets or sets a collection of available airplanes for selection. Nullable.
        /// </summary>
        public IEnumerable<AirplaneComboViewModel>? Airplanes { get; set; }

    }
}