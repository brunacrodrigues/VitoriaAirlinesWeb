using System.ComponentModel.DataAnnotations;
using VitoriaAirlinesWeb.Data.Entities;
using VitoriaAirlinesWeb.Data.Enums;
using VitoriaAirlinesWeb.Models.Airports;

namespace VitoriaAirlinesWeb.Models.ViewModels.FlightSearch
{
    public class SearchFlightViewModel
    {
        [Display(Name = "Origin")]
        public int? OriginAirportId { get; set; }


        [Display(Name = "Destination")]
        public int? DestinationAirportId { get; set; }


        [DataType(DataType.Date)]
        [Display(Name = "Departure Date")]
        public DateTime? DepartureDate { get; set; }


        [DataType(DataType.Date)]
        [Display(Name = "Return Date")]
        public DateTime? ReturnDate { get; set; }

        public TripType TripType { get; set; } = TripType.OneWay;

        public IEnumerable<AirportDropdownViewModel>? Airports { get; set; }


        public IEnumerable<Flight> OneWayFlights { get; set; }

        public IEnumerable<Flight> ReturnFlights { get; set; }

        public HashSet<int> BookedFlightIds { get; set; }

        public bool HasSearched { get; set; }


    }
}
