using System.ComponentModel.DataAnnotations;

namespace VitoriaAirlinesAPI.Dtos
{
    public class FlightSearchRequestDto
    {
        [Required(ErrorMessage = "Origin airport is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid origin airport.")]
        public int OriginAirportId { get; set; }

        [Required(ErrorMessage = "Destination airport is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid destination airport.")]
        public int DestinationAirportId { get; set; }

        [Required(ErrorMessage = "Departure date is required.")]
        public DateTime DepartureDate { get; set; }

        public DateTime? ReturnDate { get; set; } // Optional, only for round trips

        public bool IsRoundTrip { get; set; } = false;

        [Required(ErrorMessage = "Number of passengers is required.")]
        [Range(1, 10, ErrorMessage = "Number of passengers must be between 1 and 10.")] // Example range
        public int NumberOfPassengers { get; set; } = 1;
    }
}
