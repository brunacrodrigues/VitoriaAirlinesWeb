using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VitoriaAirlinesWeb.Data.Entities;
using VitoriaAirlinesWeb.Data.Enums;
using VitoriaAirlinesWeb.Data.Repositories;
using VitoriaAirlinesWeb.Helpers;
using VitoriaAirlinesWeb.Models.ViewModels.Flights;
using VitoriaAirlinesWeb.Services;

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
        private readonly INotificationService _notificationService;
        private readonly ITicketRepository _ticketRepository;
        private readonly IMailHelper _mailHelper;

        public FlightsController(
            IFlightRepository flightRepository,
            IConverterHelper converterHelper,
            IAirplaneRepository airplaneRepository,
            IAirportRepository airportRepository,
            IFlightHelper flightHelper,
            INotificationService notificationService,
            ITicketRepository ticketRepository,
            IMailHelper mailHelper)
        {
            _flightRepository = flightRepository;
            _converterHelper = converterHelper;
            _airplaneRepository = airplaneRepository;
            _airportRepository = airportRepository;
            _flightHelper = flightHelper;
            _notificationService = notificationService;
            _ticketRepository = ticketRepository;
            _mailHelper = mailHelper;
        }


        // GET: FlightsController
        public IActionResult Index()
        {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Lisbon");

            var flights = _flightRepository.GetAllWithDetailsAsync();

            var model = _converterHelper.ToFlightDisplayViewModel(flights);

            return View(model);
        }


        // GET: FlightsController/Details/5
        public async Task<IActionResult> Details(int id, string? returnUrl = null)
        {
            var flight = await _flightRepository.GetByIdWithDetailsAsync(id);
            if (flight == null) return new NotFoundViewResult("Error404");

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FlightViewModel viewModel, string? returnUrl)
        {
            if (viewModel.DepartureDate.HasValue && viewModel.DepartureTime.HasValue)
            {
                var localDeparture = DateTime.SpecifyKind(
    viewModel.DepartureDate.Value.ToDateTime(viewModel.DepartureTime.Value),
    DateTimeKind.Unspecified
);
                var departureUtc = TimezoneHelper.ConvertToUtc(localDeparture);


                if (departureUtc < DateTime.UtcNow)
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
                return await LoadViewModelCombos(viewModel, returnUrl);
            }

            var flightNumber = await _flightHelper.GenerateUniqueFlightNumberAsync();
            viewModel.FlightNumber = flightNumber;

            var localDepartureFinal = DateTime.SpecifyKind(
                viewModel.DepartureDate!.Value.ToDateTime(viewModel.DepartureTime!.Value),
                DateTimeKind.Unspecified
            );
            var departureUtcFinal = TimezoneHelper.ConvertToUtc(localDepartureFinal);


            var isAvailable = await _flightRepository.IsAirplaneAvailableAsync(
                viewModel.AirplaneId,
                departureUtcFinal,
                viewModel.Duration!.Value,
                viewModel.OriginAirportId!.Value
            );

            if (!isAvailable)
            {
                ModelState.AddModelError("", "The selected airplane is either unavailable or not at the origin airport at the scheduled time.");
                return await LoadViewModelCombos(viewModel, returnUrl);
            }

            var flight = _converterHelper.ToFlight(viewModel, isNew: true);
            await _flightRepository.CreateAsync(flight);

            flight = await _flightRepository.GetByIdWithDetailsAsync(flight.Id);
            var dashboardModel = _converterHelper.ToFlightDashboardViewModel(flight);

            await _notificationService.NotifyAdminsAsync($" New flight {flight.FlightNumber} was scheduled.");
            await _notificationService.NotifyEmployeesAsync($" Flight {flight.FlightNumber} was added to the schedule.");
            await _notificationService.NotifyNewFlightScheduledAsync(dashboardModel);

            TempData["SuccessMessage"] = "Flight scheduled successfully.";
            return Redirect(returnUrl ?? Url.Action(nameof(Index)));
        }



        // GET: FlightsController/Edit/5
        public async Task<IActionResult> Edit(int id, string? returnUrl = null)
        {
            var flight = await _flightRepository.GetByIdWithDetailsAsync(id);
            if (flight == null) return new NotFoundViewResult("Error404");

            if (flight.Status != FlightStatus.Scheduled)
            {
                TempData["WarningMessage"] = "Only scheduled flights can be edited.";
                return Redirect(returnUrl ?? Url.Action(nameof(Index)));
            }

            var model = _converterHelper.ToFlightViewModel(flight);

            return await LoadViewModelCombos(model, returnUrl);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(FlightViewModel viewModel, string? returnUrl)
        {
            if (viewModel.DepartureDate.HasValue && viewModel.DepartureTime.HasValue)
            {
                var localDeparture = DateTime.SpecifyKind(
    viewModel.DepartureDate.Value.ToDateTime(viewModel.DepartureTime.Value),
    DateTimeKind.Unspecified
);
                var departureUtc = TimezoneHelper.ConvertToUtc(localDeparture);


                if (departureUtc < DateTime.UtcNow)
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
                return await LoadViewModelCombos(viewModel, returnUrl);
            }

            var existingFlight = await _flightRepository.GetByIdWithDetailsAsync(viewModel.Id);
            if (existingFlight == null)
            {
                return new NotFoundViewResult("Error404");
            }

            if (existingFlight.Status != FlightStatus.Scheduled)
            {
                TempData["WarningMessage"] = "Only scheduled flights can be edited.";
                return Redirect(returnUrl ?? Url.Action(nameof(Index)));
            }

            var soldTickets = (await _ticketRepository
                .GetTicketsByFlightAsync(viewModel.Id))
                .Count(t => !t.IsCanceled);


            if (soldTickets > 0 &&
                (existingFlight.OriginAirportId != viewModel.OriginAirportId ||
                existingFlight.DestinationAirportId != viewModel.DestinationAirportId))
            {
                //ModelState.AddModelError(string.Empty, "Cannot change origin or destination for flights with sold tickets.");
                TempData["WarningMessage"] = "This flight already has tickets sold. You cannot change the origin or destination.";
                return await LoadViewModelCombos(viewModel, returnUrl);
            }

            var originalDepartureUtc = existingFlight.DepartureUtc;

            var newDepartureUtc = TimezoneHelper.ConvertToUtc(
                DateTime.SpecifyKind(
                    viewModel.DepartureDate!.Value.ToDateTime(viewModel.DepartureTime!.Value),
                    DateTimeKind.Unspecified
                )
            );

            if (soldTickets > 0 && originalDepartureUtc != newDepartureUtc)
            {
                TempData["WarningMessage"] = "This flight has sold tickets — departure date and time cannot be changed.";
                return await LoadViewModelCombos(viewModel, returnUrl);

            }

            var newAirplane = await _airplaneRepository
                .GetByIdWithSeatsAsync(viewModel.AirplaneId);
            var newCapacity = newAirplane?.Seats.Count ?? 0;

            if (newCapacity < soldTickets)
            {
                ModelState.AddModelError(
                    string.Empty,
                    $"Selected airplane has only {newCapacity} seats, but there are {soldTickets} tickets sold. " +
                    "Please choose an aircraft with equal or greater capacity.");
                return await LoadViewModelCombos(viewModel, returnUrl);
            }


            var localDepartureFinal = DateTime.SpecifyKind(
    viewModel.DepartureDate!.Value.ToDateTime(viewModel.DepartureTime!.Value),
    DateTimeKind.Unspecified
);
            var departureUtcFinal = TimezoneHelper.ConvertToUtc(localDepartureFinal);


            var isAvailable = await _flightRepository.IsAirplaneAvailableAsync(
                viewModel.AirplaneId,
                departureUtcFinal,
                viewModel.Duration!.Value,
                viewModel.OriginAirportId!.Value,
                viewModel.Id // exclude self
            );

            if (!isAvailable)
            {
                ModelState.AddModelError(string.Empty, "The selected airplane is either unavailable or not at the origin airport at the scheduled time.");
                return await LoadViewModelCombos(viewModel, returnUrl);
            }

            var oldAirplaneId = existingFlight.AirplaneId;
            _converterHelper.UpdateFlightFromViewModel(existingFlight, viewModel);
            await _flightRepository.UpdateAsync(existingFlight);

            if (oldAirplaneId != existingFlight.AirplaneId)
            {
                await ReassignTicketsForFlightAsync(existingFlight.Id, existingFlight.FlightNumber);
            }

            await _notificationService.NotifyAdminsAsync($"Flight {existingFlight.FlightNumber} has been updated.");
            await _notificationService.NotifyEmployeesAsync($"Flight {existingFlight.FlightNumber} was updated by staff");
            await _notificationService.NotifyFlightCustomersAsync(existingFlight, $"Your flight {existingFlight.FlightNumber} has been updated. Please review the new details.");

            var fullFlight = await _flightRepository.GetByIdWithDetailsAsync(existingFlight.Id);

            var dashboardModel = _converterHelper.ToFlightDashboardViewModel(fullFlight);
            await _notificationService.NotifyUpdatedFlightDashboardAsync(dashboardModel);

            TempData["SuccessMessage"] = "Flight updated successfully.";
            return Redirect(returnUrl ?? Url.Action(nameof(Index)));
        }


        // POST: FlightsController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id, string? returnUrl)
        {
            var flight = await _flightRepository.GetByIdWithDetailsAsync(id);
            if (flight == null)
                return new NotFoundViewResult("Error404");


            if (flight.Status != FlightStatus.Scheduled)
            {
                TempData["WarningMessage"] = "Only scheduled flights can be canceled.";
                return Redirect(returnUrl ?? Url.Action("Index", "Flights"));
            }


            flight.Status = FlightStatus.Canceled;
            await _flightRepository.UpdateAsync(flight);

            TempData["SuccessMessage"] = "Flight canceled successfully.";


            await _notificationService.NotifyAdminsAsync($"Flight {flight.FlightNumber} was canceled by a staff member.");
            await _notificationService.NotifyFlightCustomersAsync(flight, $"Your flight {flight.FlightNumber} has been canceled. You will be issued with a refund.");

            await _notificationService.NotifyFlightStatusChangedAsync(flight.Id, "Canceled");


            var tickets = flight.Tickets
                .Where(t => !t.IsCanceled && t.User != null)
                .ToList();

            foreach (var ticket in tickets)
            {
                var message = $"Dear {ticket.User.FullName},<br/><br/>" +
                              $"We regret to inform you that your flight <strong>{flight.FlightNumber}</strong> scheduled from " +
                              $"<strong>{flight.OriginAirport.City}</strong> to <strong>{flight.DestinationAirport.City}</strong> has been <strong>canceled</strong>.<br/><br/>" +
                              $"You will be issued a full refund automatically.<br/><br/>" +
                              $"We apologize for the inconvenience.<br/><br/>" +
                              $"Sincerely,<br/>Vitoria Airlines";

                await _mailHelper.SendEmailAsync(
                    ticket.User.Email,
                    $"Flight {flight.FlightNumber} Canceled",
                    message
                );
            }


            return Redirect(returnUrl ?? Url.Action("Index", "Flights"));
        }


        [HttpGet]
        public async Task<IActionResult> History()
        {
            var flights = await _flightRepository.GetFlightsHistoryAsync();
            var model = _converterHelper.ToFlightDisplayViewModel(flights);
            return View(model);
        }



        [HttpGet]
        public async Task<IActionResult> Scheduled()
        {
            var flights = await _flightRepository.GetScheduledFlightsAsync();
            var model = _converterHelper.ToFlightDisplayViewModel(flights);
            return View(model);
        }


        private async Task<IActionResult> LoadViewModelCombos(FlightViewModel viewModel, string? returnUrl)
        {
            viewModel.Airplanes = await _airplaneRepository.GetComboAirplanesAsync();
            var airports = await _airportRepository.GetComboAirportsWithFlagsAsync();
            viewModel.OriginAirports = airports;
            viewModel.DestinationAirports = airports;

            ViewBag.ReturnUrl = returnUrl ?? Url.Action("Index", "Flights");
            return View(viewModel);
        }




        private async Task ReassignTicketsForFlightAsync(int flightId, string flightNumber)
        {
            // 1) Load flight with seats
            var flight = await _flightRepository.GetByIdWithAirplaneAndSeatsAsync(flightId);
            if (flight == null) return;

            // 2) Get all non-canceled tickets for this flight
            var tickets = (await _ticketRepository.GetTicketsByFlightAsync(flightId))
                .Where(t => !t.IsCanceled)
                .OrderBy(t => t.Id)
                .ToList();

            // 3) Build two ordered seat lists
            var execSeats = flight.Airplane.Seats
                .Where(s => s.Class == SeatClass.Executive)
                .OrderBy(s => s.Row).ThenBy(s => s.Letter)
                .Select(s => s.Id)
                .ToList();

            var econSeats = flight.Airplane.Seats
                .Where(s => s.Class == SeatClass.Economy)
                .OrderBy(s => s.Row).ThenBy(s => s.Letter)
                .Select(s => s.Id)
                .ToList();

            int execIndex = 0, econIndex = 0;

            // 4) Reassign each ticket, preserving its original class
            foreach (var ticket in tickets)
            {
                var originalClass = ticket.Seat.Class;
                int? newSeatId = null;

                if (originalClass == SeatClass.Executive && execIndex < execSeats.Count)
                    newSeatId = execSeats[execIndex++];
                if (originalClass == SeatClass.Economy && econIndex < econSeats.Count)
                    newSeatId = econSeats[econIndex++];

                // only update if we got a seat
                if (newSeatId.HasValue)
                {
                    ticket.SeatId = newSeatId.Value;
                    await _ticketRepository.UpdateAsync(ticket);

                    var seatInfo = flight.Airplane.Seats
                           .First(s => s.Id == newSeatId.Value);

                    await NotifyPassengerAsync(ticket, flightNumber, seatInfo);
                }
            }
        }



        private async Task NotifyPassengerAsync(Ticket ticket, string flightNumber, Seat seatInfo)
        {
            var position = $"{seatInfo.Row}{seatInfo.Letter}";
            var cls = seatInfo.Class;


            await _notificationService.NotifyCustomerAsync(
                ticket.UserId,
                $"Your seat on flight {flightNumber} has been changed to {position} (Class: {cls})."
            );


            var user = ticket.User;
            var subject = $"Seat change on flight {flightNumber}";
            var body = $@"
                            Hello {user.FullName},<br/><br/>
                            Due to an aircraft change, your seat on flight <strong>{flightNumber}</strong> 
                            has been reassigned to <strong>{position}</strong> in <strong>{cls}</strong> class.<br/><br/>
                            We apologize for any inconvenience.<br/>
                            Safe travels!<br/>
                            Vitoria Airlines
                        ";
            await _mailHelper.SendEmailAsync(user.Email, subject, body);
        }

    }
}
