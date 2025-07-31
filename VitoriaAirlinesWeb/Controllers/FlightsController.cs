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
    /// <summary>
    /// Manages flight operations, including creating, viewing, editing, and canceling flights.
    /// Only accessible by users with the Employee role.
    /// </summary>
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

        /// <summary>
        /// Initializes a new instance of the FlightsController with necessary repositories and helpers.
        /// </summary>
        /// <param name="flightRepository">Repository for flight data access.</param>
        /// <param name="converterHelper">Helper for converting between entities and view models.</param>
        /// <param name="airplaneRepository">Repository for airplane data access.</param>
        /// <param name="airportRepository">Repository for airport data access.</param>
        /// <param name="flightHelper">Helper for flight-specific utilities.</param>
        /// <param name="notificationService">Service for sending notifications.</param>
        /// <param name="ticketRepository">Repository for ticket data access.</param>
        /// <param name="mailHelper">Helper for sending emails.</param>
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



        /// <summary>
        /// Displays a list of all flights with details.
        /// </summary>
        /// <returns>
        /// IActionResult: A view displaying a collection of flight display view models.
        /// </returns>
        // GET: FlightsController
        public IActionResult Index()
        {
            // Note: timeZone variable is declared but not used.
            // var timeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Lisbon"); 

            var flights = _flightRepository.GetAllWithDetailsAsync();

            var model = _converterHelper.ToFlightDisplayViewModel(flights);

            return View(model);
        }



        /// <summary>
        /// Displays the details of a specific flight.
        /// </summary>
        /// <param name="id">The ID of the flight.</param>
        /// <param name="returnUrl">Optional: The URL to return to after viewing details.</param>
        /// <returns>
        /// Task: A view displaying the flight details, or a 404 error if not found.
        /// </returns>
        // GET: FlightsController/Details/5
        public async Task<IActionResult> Details(int id, string? returnUrl = null)
        {
            var flight = await _flightRepository.GetByIdWithDetailsAsync(id);
            if (flight == null) return new NotFoundViewResult("Error404");

            ViewBag.ReturnUrl = returnUrl ?? Url.Action("Index", "Flights");

            return View(flight);
        }



        /// <summary>
        /// Displays the form to create a new flight.
        /// </summary>
        /// <param name="returnUrl">Optional: The URL to return to after creating the flight.</param>
        /// <returns>
        /// Task: The create flight view, pre-populated with airport and airplane options.
        /// </returns>
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



        /// <summary>
        /// Handles the submission of the new flight creation form.
        /// Validates input, generates a unique flight number, checks airplane availability, creates the flight, and sends notifications.
        /// </summary>
        /// <param name="viewModel">The flight data from the form.</param>
        /// <param name="returnUrl">Optional: The URL to return to after creating the flight.</param>
        /// <returns>
        /// Task: Redirects to the Index on success, or returns the view with validation errors.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FlightViewModel viewModel, string? returnUrl)
        {
            if (!ValidateFlightViewModel(viewModel)) // Consolidated validation
            {
                return await LoadViewModelCombos(viewModel, returnUrl);
            }

            var localDeparture = DateTime.SpecifyKind(
                viewModel.DepartureDate!.Value.ToDateTime(viewModel.DepartureTime!.Value),
                DateTimeKind.Unspecified
            );
            var departureUtc = TimezoneHelper.ConvertToUtc(localDeparture);

            if (!await CheckAirplaneAvailability(viewModel.AirplaneId, departureUtc, viewModel.Duration!.Value, viewModel.OriginAirportId!.Value)) // Consolidated availability check
            {
                ModelState.AddModelError("", "The selected airplane is either unavailable or not at the origin airport at the scheduled time.");
                return await LoadViewModelCombos(viewModel, returnUrl);
            }

            await HandleFlightCreation(viewModel);

            TempData["SuccessMessage"] = "Flight scheduled successfully.";
            return Redirect(returnUrl ?? Url.Action(nameof(Index)));
        }



        /// <summary>
        /// Displays the form to edit an existing flight.
        /// </summary>
        /// <param name="id">The ID of the flight to edit.</param>
        /// <param name="returnUrl">Optional: The URL to return to after editing.</param>
        /// <returns>
        /// Task: The edit flight view, pre-populated with flight data and dropdown options, or a 404 error if not found.
        /// </returns>
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



        /// <summary>
        /// Handles the submission of the flight edit form.
        /// Validates input, applies business rules for changes (e.g., if tickets are sold), updates the flight, and sends notifications.
        /// </summary>
        /// <param name="viewModel">The updated flight data from the form.</param>
        /// <param name="returnUrl">Optional: The URL to return to after editing the flight.</param>
        /// <returns>
        /// Task: Redirects to the Index on success, or returns the view with validation/error messages.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(FlightViewModel viewModel, string? returnUrl)
        {
            if (!ValidateFlightViewModel(viewModel)) // Consolidated validation
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

            var soldTicketsCount = (await _ticketRepository.GetTicketsByFlightAsync(viewModel.Id)).Count(t => !t.IsCanceled);

            // Consolidated edit restrictions checks
            var restrictionResult = await CheckFlightEditRestrictions(viewModel, existingFlight, soldTicketsCount, returnUrl);
            if (restrictionResult != null)
            {
                return restrictionResult;
            }

            await HandleFlightUpdate(viewModel, existingFlight, soldTicketsCount); // Consolidated update logic

            TempData["SuccessMessage"] = "Flight updated successfully.";
            return Redirect(returnUrl ?? Url.Action(nameof(Index)));
        }



        /// <summary>
        /// Handles the cancellation of a flight.
        /// Changes the flight status to Canceled, sends notifications to admins, customers, and reassigns tickets.
        /// </summary>
        /// <param name="id">The ID of the flight to cancel.</param>
        /// <param name="returnUrl">Optional: The URL to return to after cancellation.</param>
        /// <returns>
        /// Task: Redirects to the Index with a success/warning message.
        /// </returns>
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
                var message = $"Dear {ticket.User!.FullName},<br/><br/>" + // Null-conditional operator for ticket.User
                              $"We regret to inform you that your flight <strong>{flight.FlightNumber}</strong> scheduled from " +
                              $"<strong>{flight.OriginAirport!.City}</strong> to <strong>{flight.DestinationAirport!.City}</strong> has been <strong>canceled</strong>.<br/><br/>" + // Null-conditional for Airport City
                              $"You will be issued a full refund automatically.<br/><br/>" +
                              $"We apologize for the inconvenience.<br/><br/>" +
                              $"Sincerely,<br/>Vitoria Airlines";

                await _mailHelper.SendEmailAsync(
                    ticket.User.Email!, // Null-forgiving operator as Email is required
                    $"Flight {flight.FlightNumber} Canceled",
                    message
                );
            }


            return Redirect(returnUrl ?? Url.Action("Index", "Flights"));
        }



        /// <summary>
        /// Displays a history of past and canceled flights.
        /// </summary>
        /// <returns>
        /// Task: A view displaying a collection of flight display view models.
        /// </returns>
        [HttpGet]
        public async Task<IActionResult> History()
        {
            var flights = await _flightRepository.GetFlightsHistoryAsync();
            var model = _converterHelper.ToFlightDisplayViewModel(flights);
            return View(model);
        }



        /// <summary>
        /// Displays a list of currently scheduled future flights.
        /// </summary>
        /// <returns>
        /// Task: A view displaying a collection of flight display view models.
        /// </returns>
        [HttpGet]
        public async Task<IActionResult> Scheduled()
        {
            var flights = await _flightRepository.GetScheduledFlightsAsync();
            var model = _converterHelper.ToFlightDisplayViewModel(flights);
            return View(model);
        }



        #region Private Helper Methods for Refactoring

        /// <summary>
        /// Loads necessary combo box data (airports, airplanes) into the FlightViewModel.
        /// </summary>
        /// <param name="viewModel">The FlightViewModel to populate.</param>
        /// <param name="returnUrl">The URL to set for ViewBag.ReturnUrl.</param>
        /// <returns>Task: The ViewResult with the populated viewModel.</returns>
        private async Task<IActionResult> LoadViewModelCombos(FlightViewModel viewModel, string? returnUrl)
        {
            viewModel.Airplanes = await _airplaneRepository.GetComboAirplanesAsync();
            var airports = await _airportRepository.GetComboAirportsWithFlagsAsync();
            viewModel.OriginAirports = airports;
            viewModel.DestinationAirports = airports;

            ViewBag.ReturnUrl = returnUrl ?? Url.Action("Index", "Flights");
            return View(viewModel);
        }



        /// <summary>
        /// Validates common properties of FlightViewModel (date/time, duration, price comparison).
        /// Adds validation errors to ModelState if issues are found.
        /// </summary>
        /// <param name="viewModel">The FlightViewModel to validate.</param>
        /// <returns>True if the view model is valid, false otherwise.</returns>
        private bool ValidateFlightViewModel(FlightViewModel viewModel)
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

            return ModelState.IsValid;
        }



        /// <summary>
        /// Checks if an airplane is available and correctly positioned for a new or updated flight.
        /// </summary>
        /// <param name="airplaneId">The ID of the airplane.</param>
        /// <param name="departureUtc">The proposed departure time in UTC.</param>
        /// <param name="duration">The proposed flight duration.</param>
        /// <param name="originAirportId">The ID of the origin airport.</param>
        /// <param name="flightToExcludeId">Optional: The ID of a flight to exclude from availability checks (for edits).</param>
        /// <returns>Task: True if the airplane is available, false otherwise.</returns>
        private async Task<bool> CheckAirplaneAvailability(
            int airplaneId, DateTime departureUtc, TimeSpan duration, int originAirportId, int? flightToExcludeId = null)
        {
            return await _flightRepository.IsAirplaneAvailableAsync(
                airplaneId,
                departureUtc,
                duration,
                originAirportId,
                flightToExcludeId
            );
        }



        /// <summary>
        /// Handles the creation of a new flight, including generating its flight number and sending notifications.
        /// </summary>
        /// <param name="viewModel">The FlightViewModel containing data for the new flight.</param>
        /// <returns>Task: A Task representing the asynchronous operation.</returns>
        private async Task HandleFlightCreation(FlightViewModel viewModel)
        {
            var flightNumber = await _flightHelper.GenerateUniqueFlightNumberAsync();
            viewModel.FlightNumber = flightNumber;

            var flight = _converterHelper.ToFlight(viewModel, isNew: true);
            await _flightRepository.CreateAsync(flight);

            // Re-fetch flight with details for dashboard model
            flight = await _flightRepository.GetByIdWithDetailsAsync(flight.Id);
            var dashboardModel = _converterHelper.ToFlightDashboardViewModel(flight!); // Null-forgiving as it was just created

            await _notificationService.NotifyAdminsAsync($" New flight {flight.FlightNumber} was scheduled.");
            await _notificationService.NotifyEmployeesAsync($" Flight {flight.FlightNumber} was added to the schedule.");
            await _notificationService.NotifyNewFlightScheduledAsync(dashboardModel);
        }



        /// <summary>
        /// Checks business rules and restrictions when editing a flight, especially if tickets are sold.
        /// </summary>
        /// <param name="viewModel">The updated FlightViewModel.</param>
        /// <param name="existingFlight">The existing Flight entity.</param>
        /// <param name="soldTicketsCount">The number of non-canceled sold tickets for this flight.</param>
        /// <param name="returnUrl">The URL to return to if a restriction prevents the update.</param>
        /// <returns>Task: An IActionResult if a restriction is met (e.g., redirects with warning), otherwise null.</returns>
        private async Task<IActionResult?> CheckFlightEditRestrictions(
            FlightViewModel viewModel, Flight existingFlight, int soldTicketsCount, string? returnUrl)
        {
            if (soldTicketsCount > 0)
            {
                if (existingFlight.OriginAirportId != viewModel.OriginAirportId ||
                    existingFlight.DestinationAirportId != viewModel.DestinationAirportId)
                {
                    TempData["WarningMessage"] = "This flight already has tickets sold. You cannot change the origin or destination.";
                    return await LoadViewModelCombos(viewModel, returnUrl);
                }

                var newDepartureUtc = TimezoneHelper.ConvertToUtc(
                    DateTime.SpecifyKind(
                        viewModel.DepartureDate!.Value.ToDateTime(viewModel.DepartureTime!.Value),
                        DateTimeKind.Unspecified
                    )
                );

                if (existingFlight.DepartureUtc != newDepartureUtc)
                {
                    TempData["WarningMessage"] = "This flight has sold tickets — departure date and time cannot be changed.";
                    return await LoadViewModelCombos(viewModel, returnUrl);
                }
            }

            var newAirplane = await _airplaneRepository.GetByIdWithSeatsAsync(viewModel.AirplaneId);
            var newCapacity = newAirplane?.Seats?.Count ?? 0; // Null-conditional for Seats

            if (newCapacity < soldTicketsCount)
            {
                ModelState.AddModelError(
                    string.Empty,
                    $"Selected airplane has only {newCapacity} seats, but there are {soldTicketsCount} tickets sold. " +
                    "Please choose an aircraft with equal or greater capacity.");
                return await LoadViewModelCombos(viewModel, returnUrl);
            }

            var localDepartureFinal = DateTime.SpecifyKind(
                viewModel.DepartureDate!.Value.ToDateTime(viewModel.DepartureTime!.Value),
                DateTimeKind.Unspecified
            );
            var departureUtcFinal = TimezoneHelper.ConvertToUtc(localDepartureFinal);

            if (!await CheckAirplaneAvailability(viewModel.AirplaneId, departureUtcFinal, viewModel.Duration!.Value, viewModel.OriginAirportId!.Value, viewModel.Id))
            {
                ModelState.AddModelError(string.Empty, "The selected airplane is either unavailable or not at the origin airport at the scheduled time.");
                return await LoadViewModelCombos(viewModel, returnUrl);
            }

            return null;
        }



        /// <summary>
        /// Handles the update of an existing flight, including saving changes, reassigning tickets if the airplane changed, and sending notifications.
        /// </summary>
        /// <param name="viewModel">The updated FlightViewModel.</param>
        /// <param name="existingFlight">The original Flight entity before updates.</param>
        /// <param name="soldTicketsCount">The number of non-canceled sold tickets for this flight.</param>
        /// <returns>Task: A Task representing the asynchronous operation.</returns>
        private async Task HandleFlightUpdate(FlightViewModel viewModel, Flight existingFlight, int soldTicketsCount)
        {
            var oldAirplaneId = existingFlight.AirplaneId;
            _converterHelper.UpdateFlightFromViewModel(existingFlight, viewModel);
            await _flightRepository.UpdateAsync(existingFlight);

            // Reassign tickets only if the airplane has changed and there are sold tickets.
            // Note: The original logic in the provided code snippet reassigns tickets if oldAirplaneId != existingFlight.AirplaneId
            // regardless of soldTicketsCount. If a reassign *only* happens if tickets are sold AND airplane changed,
            // then `soldTicketsCount > 0` should be added to this condition.
            if (oldAirplaneId != existingFlight.AirplaneId)
            {
                await ReassignTicketsForFlightAsync(existingFlight.Id, existingFlight.FlightNumber);
            }

            // Send notifications about the update.
            await _notificationService.NotifyAdminsAsync($"Flight {existingFlight.FlightNumber} has been updated.");
            await _notificationService.NotifyEmployeesAsync($"Flight {existingFlight.FlightNumber} was updated by staff");
            await _notificationService.NotifyFlightCustomersAsync(existingFlight, $"Your flight {existingFlight.FlightNumber} has been updated. Please review the new details.");

            // Re-fetch full flight details for dashboard notification.
            var fullFlight = await _flightRepository.GetByIdWithDetailsAsync(existingFlight.Id);
            var dashboardModel = _converterHelper.ToFlightDashboardViewModel(fullFlight!); // Null-forgiving as it should exist.
            await _notificationService.NotifyUpdatedFlightDashboardAsync(dashboardModel);
        }



        /// <summary>
        /// Reassigns seats for tickets on a flight, typically after an airplane change.
        /// Attempts to keep passengers in their original class (Executive/Economy).
        /// Notifies passengers of their new seat assignments.
        /// </summary>
        /// <param name="flightId">The ID of the flight.</param>
        /// <param name="flightNumber">The flight number (for notification purposes).</param>
        /// <returns>Task: A Task representing the asynchronous operation.</returns>
        private async Task ReassignTicketsForFlightAsync(int flightId, string flightNumber)
        {
            // Load flight with seats
            var flight = await _flightRepository.GetByIdWithAirplaneAndSeatsAsync(flightId);
            if (flight == null) return;

            // Get all non-canceled tickets for this flight
            var tickets = (await _ticketRepository.GetTicketsByFlightAsync(flightId))
                .Where(t => !t.IsCanceled)
                .OrderBy(t => t.Id) // Order to ensure consistent assignment
                .ToList();

            // Build two ordered seat lists from the NEW airplane's seats
            var execSeats = flight.Airplane!.Seats // Null-forgiving as Airplane should be loaded
                .Where(s => s.Class == SeatClass.Executive)
                .OrderBy(s => s.Row).ThenBy(s => s.Letter)
                .Select(s => s.Id)
                .ToList();

            var econSeats = flight.Airplane!.Seats // Null-forgiving as Airplane should be loaded
                .Where(s => s.Class == SeatClass.Economy)
                .OrderBy(s => s.Row).ThenBy(s => s.Letter)
                .Select(s => s.Id)
                .ToList();

            int execIndex = 0, econIndex = 0;

            // Reassign each ticket, preserving its original class
            foreach (var ticket in tickets)
            {
                var originalClass = ticket.Seat!.Class; // Null-forgiving as Seat should be loaded
                int? newSeatId = null;

                if (originalClass == SeatClass.Executive && execIndex < execSeats.Count)
                    newSeatId = execSeats[execIndex++];
                else if (originalClass == SeatClass.Economy && econIndex < econSeats.Count) // Use else if to avoid double assignment if logic error in exec block
                    newSeatId = econSeats[econIndex++];
                // Handle edge case: if a passenger was Executive but no more executive seats are available,
                // or vice-versa, they might not get a newSeatId here. The current logic simply skips.
                // A more robust solution might place them in an available seat of another class or cancel their ticket.

                // only update if we got a seat
                if (newSeatId.HasValue)
                {
                    ticket.SeatId = newSeatId.Value;
                    await _ticketRepository.UpdateAsync(ticket);

                    var seatInfo = flight.Airplane.Seats // Null-forgiving as Airplane should be loaded
                           .First(s => s.Id == newSeatId.Value);

                    await NotifyPassengerAsync(ticket, flightNumber, seatInfo);
                }
            }
        }



        /// <summary>
        /// Sends a notification and email to a passenger about a seat change due to aircraft reassignment.
        /// </summary>
        /// <param name="ticket">The updated ticket entity.</param>
        /// <param name="flightNumber">The flight number.</param>
        /// <param name="seatInfo">The new seat information.</param>
        /// <returns>Task: A Task representing the asynchronous operation.</returns>
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
                            Hello {user!.FullName},<br/><br/>
                            Due to an aircraft change, your seat on flight <strong>{flightNumber}</strong> 
                            has been reassigned to <strong>{position}</strong> in <strong>{cls}</strong> class.<br/><br/>
                            We apologize for any inconvenience.<br/>
                            Safe travels!<br/>
                            Vitoria Airlines
                        ";
            await _mailHelper.SendEmailAsync(user.Email!, subject, body);
        }

        #endregion
    }
}