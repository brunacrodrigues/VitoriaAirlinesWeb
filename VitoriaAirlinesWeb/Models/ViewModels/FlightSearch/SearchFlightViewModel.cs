using System.ComponentModel.DataAnnotations;
using VitoriaAirlinesWeb.Data.Entities;
using VitoriaAirlinesWeb.Data.Enums;
using VitoriaAirlinesWeb.Models.Airports;

namespace VitoriaAirlinesWeb.Models.ViewModels.FlightSearch
{
    /// <summary>
    /// Represents the view model for searching flights on the home page.
    /// Includes search criteria, flight results, and booked flight IDs.
    /// </summary>
    public class SearchFlightViewModel
    {
        /// <summary>
        /// Gets or sets the ID of the selected origin airport for the search. Nullable.
        /// </summary>
        [Display(Name = "Origin")]
        public int? OriginAirportId { get; set; }


        /// <summary>
        /// Gets or sets the ID of the selected destination airport for the search. Nullable.
        /// </summary>
        [Display(Name = "Destination")]
        public int? DestinationAirportId { get; set; }


        /// <summary>
        /// Gets or sets the selected departure date for the search. Nullable and expected to be a date.
        /// </summary>
        [DataType(DataType.Date)]
        [Display(Name = "Departure Date")]
        public DateTime? DepartureDate { get; set; }


        /// <summary>
        /// Gets or sets the selected return date for the search. Nullable and expected to be a date.
        /// </summary>
        [DataType(DataType.Date)]
        [Display(Name = "Return Date")]
        public DateTime? ReturnDate { get; set; }


        /// <summary>
        /// Gets or sets the type of trip being searched (OneWay or RoundTrip). Defaults to OneWay.
        /// </summary>
        public TripType TripType { get; set; } = TripType.OneWay;


        /// <summary>
        /// Gets or sets a collection of available airports for selection in dropdowns. Nullable.
        /// </summary>
        public IEnumerable<AirportDropdownViewModel>? Airports { get; set; }


        /// <summary>
        /// Gets or sets a collection of flights found for the one-way (outbound) journey.
        /// </summary>
        public IEnumerable<Flight> OneWayFlights { get; set; }


        /// <summary>
        /// Gets or sets a collection of flights found for the return journey (for round trips).
        /// </summary>
        public IEnumerable<Flight> ReturnFlights { get; set; }


        /// <summary>
        /// Gets or sets a hash set of flight IDs that the current customer has already booked.
        /// </summary>
        public HashSet<int> BookedFlightIds { get; set; }


        /// <summary>
        /// Gets or sets a flag indicating whether a search has been performed.
        /// </summary>
        public bool HasSearched { get; set; }


    }
}