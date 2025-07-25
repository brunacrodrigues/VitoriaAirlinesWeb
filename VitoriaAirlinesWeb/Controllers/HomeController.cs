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
    public class HomeController : Controller
    {
        private readonly IFlightRepository _flightRepository;
        private readonly IAirportRepository _airportRepository;
        private readonly ITicketRepository _ticketRepository;
        private readonly IUserHelper _userHelper;

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

        [HttpGet]
        public async Task<IActionResult> Index(SearchFlightViewModel viewModel)
        {
            viewModel.Airports = await _airportRepository.GetComboAirportsWithFlagsAsync();
            viewModel.OneWayFlights = new List<Flight>();
            viewModel.ReturnFlights = new List<Flight>();
            viewModel.BookedFlightIds = new HashSet<int>();

            // TODO ALTERAR ISTO DPS

            if (viewModel.TripType == TripType.RoundTrip)
            {
                if (!viewModel.OriginAirportId.HasValue || !viewModel.DestinationAirportId.HasValue)
                {
                    TempData["Error"] = "For round-trip searches, both origin and destination must be selected.";
                    return View(viewModel);
                }
            }
            else
            {
                if (!viewModel.OriginAirportId.HasValue && !viewModel.DestinationAirportId.HasValue && !viewModel.DepartureDate.HasValue)
                {
                    TempData["Error"] = "Please fill at least one field to search for flights.";
                    return View(viewModel);
                }
            }


            viewModel.OneWayFlights = await _flightRepository.SearchFlightsAsync(
                viewModel.DepartureDate,
                viewModel.OriginAirportId,
                viewModel.DestinationAirportId
            );

            if (viewModel.TripType == TripType.RoundTrip)
            {
                var returnSearchDate = viewModel.ReturnDate ?? viewModel.DepartureDate?.AddDays(1);

                viewModel.ReturnFlights = await _flightRepository.SearchFlightsAsync(
                    returnSearchDate,
                    viewModel.DestinationAirportId,
                    viewModel.OriginAirportId
                );
            }


            if (User.IsInRole(UserRoles.Customer))
            {
                var user = await _userHelper.GetUserAsync(User);
                if (user is null) return new NotFoundViewResult("Error404");

                foreach (var flight in viewModel.OneWayFlights.Concat(viewModel.ReturnFlights))
                {
                    if (await _ticketRepository.UserHasTicketForFlightAsync(user.Id, flight.Id))
                        viewModel.BookedFlightIds.Add(flight.Id);
                }
            }

            return View(viewModel);
        }



        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


        [HttpPost]
        public IActionResult SetReturnFlightId(int returnFlightId)
        {
            HttpContext.Session.Remove("OutboundFlightId");
            HttpContext.Session.Remove("OutboundSeatId");

            HttpContext.Session.SetInt32("ReturnFlightId", returnFlightId);
            return Ok(); // Retorna um status 200 OK, sem conteúdo JSON
        }

    }
}
