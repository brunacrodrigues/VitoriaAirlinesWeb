using System.ComponentModel.DataAnnotations;
using VitoriaAirlinesWeb.Data.Enums;
using VitoriaAirlinesWeb.Models.Airplanes;
using VitoriaAirlinesWeb.Models.Airports;

namespace VitoriaAirlinesWeb.Models.Flights
{
    public class FlightViewModel
    {
        public int Id { get; set; }


        [Display(Name = "Flight Number")]
        public string? FlightNumber { get; set; }


        [Required(ErrorMessage = "You must select the origin airport.")]
        public int? OriginAirportId { get; set; }


        [Required(ErrorMessage = "You must select the destination airport.")]
        public int? DestinationAirportId { get; set; }


        [Display(Name = "Airplane Model")]
        [Range(1, int.MaxValue, ErrorMessage = "You must select an airplane model.")]
        public int AirplaneId { get; set; }



        [Required(ErrorMessage = "Economy Class price is required.")]
        [Display(Name = "Economy Class Price")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Economy price must be greater than 0.")]
        [DisplayFormat(DataFormatString = "{0:C2}", ApplyFormatInEditMode = false)]
        public decimal EconomyClassPrice { get; set; }



        [Required(ErrorMessage = "Executive Class price is required.")]
        [Display(Name = "Executive Class Price")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Executive price must be greater than 0.")]
        [DisplayFormat(DataFormatString = "{0:C2}", ApplyFormatInEditMode = false)]
        public decimal ExecutiveClassPrice { get; set; }



        [Required(ErrorMessage = "Departure date is required.")]
        [Display(Name = "Departure Date")]
        [DataType(DataType.Date)]
        public DateOnly? DepartureDate { get; set; }




        [Required(ErrorMessage = "Departure time is required.")]
        [Display(Name = "Departure Time")]
        [DataType(DataType.Time)]
        public TimeOnly? DepartureTime { get; set; }




        [Required(ErrorMessage = "Flight duration is required.")]
        [Display(Name = "Flight Duration (HH:mm)")]
        public TimeSpan? Duration { get; set; }


        public FlightStatus Status { get; set; }


        public IEnumerable<AirportDropdownViewModel>? DestinationAirports { get; set; }

        public IEnumerable<AirportDropdownViewModel>? OriginAirports { get; set; }

        public IEnumerable<AirplaneComboViewModel>? Airplanes { get; set; }
    }
}
