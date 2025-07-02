using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
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

        public HomeController(
            IFlightRepository flightRepository,
            IAirportRepository airportRepository)
        {
            _flightRepository = flightRepository;
            _airportRepository = airportRepository;
        }


        [HttpGet]
        public async Task<IActionResult> Index(SearchFlightViewModel viewModel)
        {
            viewModel.Airports = await _airportRepository.GetComboAirportsWithFlagsAsync();

            if (User.IsInRole(UserRoles.Admin))
            {
                viewModel.Flights = await _flightRepository.GetScheduledFlightsAsync();
            }
            else if (viewModel.OriginAirportId.HasValue || viewModel.DestinationAirportId.HasValue || viewModel.DepartureDate.HasValue)
            {
                viewModel.Flights = await _flightRepository.SearchFlightsAsync(
                    viewModel.DepartureDate,
                    viewModel.OriginAirportId,
                    viewModel.DestinationAirportId
                );
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

        public IActionResult AssistTest()
        {
            return View();
        }
    }
}
