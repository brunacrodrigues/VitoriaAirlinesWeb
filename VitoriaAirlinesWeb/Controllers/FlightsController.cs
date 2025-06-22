using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VitoriaAirlinesWeb.Data.Enums;
using VitoriaAirlinesWeb.Data.Repositories;
using VitoriaAirlinesWeb.Helpers;
using VitoriaAirlinesWeb.Models.Flights;

namespace VitoriaAirlinesWeb.Controllers
{
    [Authorize(Roles = UserRoles.Employee)]
    public class FlightsController : Controller
    {
        private readonly IFlightRepository _flightRepository;
        private readonly IConverterHelper _converterHelper;
        private readonly IAirplaneRepository _airplaneRepository;
        private readonly IAirportRepository _airportRepository;
        private readonly IFlightHelper _flightHelper;

        public FlightsController(
            IFlightRepository flightRepository,
            IConverterHelper converterHelper,
            IAirplaneRepository airplaneRepository,
            IAirportRepository airportRepository,
            IFlightHelper flightHelper)
        {
            _flightRepository = flightRepository;
            _converterHelper = converterHelper;
            _airplaneRepository = airplaneRepository;
            _airportRepository = airportRepository;
            _flightHelper = flightHelper;
        }


        // GET: FlightsController
        public IActionResult Index()
        {
            return View(_flightRepository.GetAllWithDetailsAsync());
        }

        // GET: FlightsController/Details/5
        public async Task<IActionResult> Details(int id, string? returnUrl = null)
        {
            var flight = await _flightRepository.GetByIdWithDetailsAsync(id);
            if (flight == null) return NotFound();

            ViewBag.ReturnUrl = returnUrl ?? Url.Action("Index", "Flights");

            return View(flight);
        }

        // GET: FlightsController/Create
        public async Task<IActionResult> Create(string? returnUrl = null)
        {
            var airports = await _airportRepository.GetComboAirportsWithFlagsAsync();
            var airplanes = await _airplaneRepository.GetComboAirplanesAsync();

            var model = new FlightViewModel
            {
                OriginAirports = airports,
                DestinationAirports = airports,

                Airplanes = airplanes
            };

            ViewBag.ReturnUrl = returnUrl ?? Url.Action("Index", "Flights");

            return View(model);
        }

        // POST: FlightsController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FlightViewModel viewModel, string? returnUrl)
        {
            if (viewModel.DepartureDate.HasValue && viewModel.DepartureTime.HasValue)
            {
                var departure = viewModel.DepartureDate.Value.ToDateTime(viewModel.DepartureTime.Value);
                if (departure < DateTime.Now)
                {
                    ModelState.AddModelError(nameof(viewModel.DepartureDate), "The departure date and time cannot be in the past.");
                }
            }

            if (!viewModel.Duration.HasValue || viewModel.Duration.Value.TotalMinutes <= 0)
            {
                ModelState.AddModelError(nameof(viewModel.Duration), "Flight duration must be greater than zero.");
            }

            if (viewModel.ExecutiveClassPrice < viewModel.EconomyClassPrice)
            {
                ModelState.AddModelError(nameof(viewModel.ExecutiveClassPrice), "Executive price cannot be lower than economy price.");
            }

            if (!ModelState.IsValid)
            {
                viewModel.Airplanes = await _airplaneRepository.GetComboAirplanesAsync();
                var airports = await _airportRepository.GetComboAirportsWithFlagsAsync();
                viewModel.OriginAirports = airports;
                viewModel.DestinationAirports = airports;

                ViewBag.ReturnUrl = returnUrl ?? Url.Action("Index", "Flights");
                return View(viewModel);
            }
            var flightNumber = await _flightHelper.GenerateUniqueFlightNumberAsync();
            viewModel.FlightNumber = flightNumber;


            var flight = _converterHelper.ToFlight(viewModel, isNew: true);
            await _flightRepository.CreateAsync(flight);

            TempData["SuccessMessage"] = "Flight scheduled successfully.";
            return Redirect(returnUrl ?? Url.Action(nameof(Index)));
        }

        // GET: FlightsController/Edit/5
        public async Task<IActionResult> Edit(int id, string? returnUrl = null)
        {
            var flight = await _flightRepository.GetByIdWithDetailsAsync(id);
            if (flight == null) return NotFound();

            if (flight.Status != FlightStatus.Scheduled)
            {
                TempData["WarningMessage"] = "Only scheduled flights can be edited.";
                return Redirect(returnUrl ?? Url.Action(nameof(Index)));
            }

            var model = _converterHelper.ToFlightViewModel(flight);
            model.Airplanes = await _airplaneRepository.GetComboAirplanesAsync();
            var airports = await _airportRepository.GetComboAirportsWithFlagsAsync();
            model.OriginAirports = airports;
            model.DestinationAirports = airports;

            ViewBag.ReturnUrl = returnUrl ?? Url.Action("Index", "Flights");

            return View(model);
        }

        // POST: FlightsController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(FlightViewModel viewModel, string? returnUrl)
        {
            if (!ModelState.IsValid)
            {
                viewModel.Airplanes = await _airplaneRepository.GetComboAirplanesAsync();
                var airports = await _airportRepository.GetComboAirportsWithFlagsAsync();
                viewModel.OriginAirports = airports;
                viewModel.DestinationAirports = airports;

                ViewBag.ReturnUrl = returnUrl ?? Url.Action("Index", "Flights");
                return View(viewModel);
            }

            var existingFlight = await _flightRepository.GetByIdAsync(viewModel.Id);
            if (existingFlight == null)
            {
                return NotFound();
            }

            if (existingFlight.Status != FlightStatus.Scheduled)
            {
                TempData["WarningMessage"] = "Only scheduled flights can be edited.";
                return Redirect(returnUrl ?? Url.Action(nameof(Index)));
            }

            var updatedFlight = _converterHelper.ToFlight(viewModel, isNew: false);

            updatedFlight.Status = existingFlight.Status;

            await _flightRepository.UpdateAsync(updatedFlight);

            TempData["SuccessMessage"] = "Flight updated successfully.";
            return Redirect(returnUrl ?? Url.Action(nameof(Index)));

        }


        // GET: FlightsController/Delete/5
        public async Task<IActionResult> Cancel(int id, string? returnUrl = null)
        {
            var flight = await _flightRepository.GetByIdWithDetailsAsync(id);
            if (flight == null) return NotFound();

            if (flight.Status != FlightStatus.Scheduled)
            {
                TempData["WarningMessage"] = "Only scheduled flights can be canceled.";
                return Redirect(returnUrl ?? Url.Action("Index", "Flights"));
            }

            ViewBag.ReturnUrl = returnUrl ?? Url.Action("Index", "Flights");

            return View(flight);
        }

        // POST: FlightsController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelConfirmed(int id, string? returnUrl)
        {
            var flight = await _flightRepository.GetByIdAsync(id);
            if (flight == null)
                return NotFound();

            if (flight.Status != FlightStatus.Scheduled)
            {
                TempData["WarningMessage"] = "Only scheduled flights can be canceled.";
                return Redirect(returnUrl ?? Url.Action("Index", "Flights"));
            }

            flight.Status = FlightStatus.Canceled;
            await _flightRepository.UpdateAsync(flight);

            TempData["SuccessMessage"] = "Flight canceled successfully.";
            return Redirect(returnUrl ?? Url.Action("Index", "Flights"));
        }


        [HttpGet]
        public async Task<IActionResult> History()
        {
            var flights = await _flightRepository.GetFlightsHistoryAsync();
            return View(flights);
        }

        [HttpGet]
        public async Task<IActionResult> Scheduled()
        {
            var flights = await _flightRepository.GetScheduledFlightsAsync();
            return View(flights);
        }
    }
}
