using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using VitoriaAirlinesWeb.Data.Entities;
using VitoriaAirlinesWeb.Data.Enums;
using VitoriaAirlinesWeb.Data.Repositories;
using VitoriaAirlinesWeb.Helpers;
using VitoriaAirlinesWeb.Models.ViewModels;
using VitoriaAirlinesWeb.Models.ViewModels.FlightSearch;

namespace VitoriaAirlinesWeb.Controllers
{
    /// <summary>
    /// Handles the main public-facing pages of the application, including the flight search functionality.
    /// </summary>
    public class HomeController : Controller
    {
        private readonly IFlightRepository _flightRepository;
        private readonly IAirportRepository _airportRepository;
        private readonly ITicketRepository _ticketRepository;
        private readonly IUserHelper _userHelper;

        /// <summary>
        /// Initializes a new instance of the HomeController with necessary repositories and helpers.
        /// </summary>
        /// <param name="flightRepository">Repository for flight data access.</param>
        /// <param name="airportRepository">Repository for airport data access.</param>
        /// <param name="ticketRepository">Repository for ticket data access.</param>
        /// <param name="userHelper">Helper for user-related operations.</param>
        public HomeController(
            IFlightRepository flightRepository,
            IAirportRepository airportRepository,
            ITicketRepository ticketRepository,
            IUserHelper userHelper)
        {
            _flightRepository = flightRepository;
            _airportRepository = airportRepository;
            _ticketRepository = ticketRepository;
            _userHelper = userHelper;
        }


        /// <summary>
        /// Displays the home page with flight search functionality.
        /// Handles search queries, validates input, performs flight searches,
        /// and marks flights already booked by the current customer.
        /// </summary>
        /// <param name="viewModel">The view model containing search parameters and results.</param>
        /// <returns>
        /// Task: A view displaying the flight search interface and results.
        /// </returns>
        [HttpGet]
        public async Task<IActionResult> Index(SearchFlightViewModel viewModel)
        {
            // Clean session before a new search
            HttpContext.Session.Remove("OutboundFlightId");
            HttpContext.Session.Remove("OutboundSeatId");
            HttpContext.Session.Remove("ReturnFlightId");
            HttpContext.Session.Remove("ReturnSeatId");
            HttpContext.Session.Remove("FlightId");
            HttpContext.Session.Remove("SeatId");
            HttpContext.Session.Remove("Price");

            viewModel.Airports = await _airportRepository.GetComboAirportsWithFlagsAsync();
            viewModel.BookedFlightIds = new();
            viewModel.HasSearched = false;


            if (Request.Query.ContainsKey("TripType"))
            {
                viewModel.HasSearched = true;


                if (!viewModel.OriginAirportId.HasValue &&
                    !viewModel.DestinationAirportId.HasValue &&
                    !viewModel.DepartureDate.HasValue &&
                    !viewModel.ReturnDate.HasValue)
                {
                    TempData["Error"] = "Please fill in at least one field before searching.";
                    return View(viewModel);
                }


                if (viewModel.OriginAirportId == viewModel.DestinationAirportId &&
                    viewModel.OriginAirportId.HasValue && viewModel.DestinationAirportId.HasValue)
                {
                    TempData["Error"] = "Origin and destination airports cannot be the same.";
                    return View(viewModel);
                }

                if (viewModel.TripType == TripType.RoundTrip &&
                    (!viewModel.OriginAirportId.HasValue || !viewModel.DestinationAirportId.HasValue))
                {
                    TempData["Error"] = "For round-trip searches, both origin and destination must be selected.";
                    return View(viewModel);
                }


                viewModel.OneWayFlights = await _flightRepository.SearchFlightsAsync(
                    viewModel.DepartureDate,
                    viewModel.OriginAirportId,
                    viewModel.DestinationAirportId);

                if (viewModel.TripType == TripType.RoundTrip)
                {
                    var returnSearchDate = viewModel.ReturnDate ?? viewModel.DepartureDate?.AddDays(1);

                    viewModel.ReturnFlights = await _flightRepository.SearchFlightsAsync(
                        returnSearchDate,
                        viewModel.DestinationAirportId,
                        viewModel.OriginAirportId);
                }

                if (User.IsInRole(UserRoles.Customer))
                {
                    var user = await _userHelper.GetUserAsync(User);
                    if (user is null) return new NotFoundViewResult("Error404");

                    foreach (var flight in (viewModel.OneWayFlights ?? Enumerable.Empty<Flight>())
                        .Concat(viewModel.ReturnFlights ?? Enumerable.Empty<Flight>()))
                    {
                        if (await _ticketRepository.UserHasTicketForFlightAsync(user.Id, flight.Id))
                            viewModel.BookedFlightIds.Add(flight.Id);
                    }
                }
            }

            return View(viewModel);
        }


        /// <summary>
        /// Displays the "About Us" page.
        /// </summary>
        /// <returns>
        /// IActionResult: The About view.
        /// </returns>
        public IActionResult About()
        {
            return View();
        }


        /// <summary>
        /// Displays the "Frequently Asked Questions" (FAQs) page.
        /// </summary>
        /// <returns>
        /// IActionResult: The FAQS view.
        /// </returns>
        public IActionResult FAQS()
        {
            return View();
        }



        /// <summary>
        /// Displays the "Privacy Policy" page.
        /// </summary>
        /// <returns>
        /// IActionResult: The Privacy view.
        /// </returns>
        public IActionResult Privacy()
        {
            return View();
        }



        /// <summary>
        /// Displays a generic error page. This action is used to show details about the request error.
        /// </summary>
        /// <returns>
        /// IActionResult: The Error view with request ID information.
        /// </returns>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}