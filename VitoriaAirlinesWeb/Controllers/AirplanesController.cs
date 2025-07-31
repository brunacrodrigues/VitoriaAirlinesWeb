using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VitoriaAirlinesWeb.Data.Entities;
using VitoriaAirlinesWeb.Data.Enums;
using VitoriaAirlinesWeb.Data.Repositories;
using VitoriaAirlinesWeb.Helpers;
using VitoriaAirlinesWeb.Models.ViewModels.Airplanes;
using VitoriaAirlinesWeb.Services;

namespace VitoriaAirlinesWeb.Controllers
{
    /// <summary>
    /// Manages airplane-related operations. Only accessible by users with the Admin role.
    /// </summary>
    [Authorize(Roles = UserRoles.Admin)]
    public class AirplanesController : Controller
    {
        private readonly IAirplaneRepository _airplaneRepository;
        private readonly IFlightRepository _flightRepository;
        private readonly ISeatGeneratorHelper _seatGeneratorHelper;
        private readonly IConverterHelper _converterHelper;
        private readonly IBlobHelper _blobHelper;
        private readonly IUserHelper _userHelper;
        private readonly IMailHelper _mailHelper;
        private readonly INotificationService _notificationService;


        /// <summary>
        /// Initializes a new instance of the AirplanesController with necessary repositories and helpers.
        /// </summary>
        /// <param name="airplaneRepository"></param>
        /// <param name="flightRepository"></param>
        /// <param name="seatGeneratorHelper"></param>
        /// <param name="converterHelper"></param>
        /// <param name="blobHelper"></param>
        /// <param name="userHelper"></param>
        /// <param name="mailHelper"></param>
        /// <param name="notificationService"></param>
        public AirplanesController(
            IAirplaneRepository airplaneRepository,
            IFlightRepository flightRepository,
            ISeatGeneratorHelper seatGeneratorHelper,
            IConverterHelper converterHelper,
            IBlobHelper blobHelper,
            IUserHelper userHelper,
            IMailHelper mailHelper,
            INotificationService notificationService)
        {
            _airplaneRepository = airplaneRepository;
            _flightRepository = flightRepository;
            _seatGeneratorHelper = seatGeneratorHelper;
            _converterHelper = converterHelper;
            _blobHelper = blobHelper;
            _userHelper = userHelper;
            _mailHelper = mailHelper;
            _notificationService = notificationService;
        }


        /// <summary>
        /// Displays a list of all airplanes, ordered by model.
        /// </summary>
        /// <returns>
        /// IActionResult: A view displaying a collection of airplanes.
        /// </returns>
        // GET: AirplanesController
        public IActionResult Index()
        {
            return View(_airplaneRepository.GetAll()
                .OrderBy(a => a.Model));
        }


        /// <summary>
        /// Displays the details of a specific airplane, including its seats.
        /// </summary>
        /// <param name="id">The ID of the airplane.</param>
        /// <returns>
        /// Task: A view displaying the airplane details, or a 404 error if not found.
        /// </returns>
        // GET: AirplanesController/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var airplane = await _airplaneRepository.GetByIdWithSeatsAsync(id);
            if (airplane == null)
                return new NotFoundViewResult("Error404");

            return View(airplane);
        }



        /// <summary>
        /// Displays the form to create a new airplane.
        /// </summary>
        /// <returns>
        /// IActionResult: The create airplane view.
        /// </returns>
        // GET: AirplanesController/Create
        public IActionResult Create()
        {
            return View();
        }



        /// <summary>
        /// Handles the submission of the new airplane creation form.
        /// Uploads an image, creates the airplane, and generates seats.
        /// </summary>
        /// <param name="viewModel">The airplane data from the form.</param>
        /// <returns>
        /// Task: Redirects to the Index on success, or returns the view with validation errors.
        /// </returns>
        // POST: AirplanesController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AirplaneViewModel viewModel)
        {
            if (viewModel.ImageFile == null)
            {
                ModelState.AddModelError(nameof(viewModel.ImageFile), "An image is required.");
            }


            if (viewModel.TotalExecutiveSeats >= viewModel.TotalEconomySeats)
            {
                ModelState.AddModelError(string.Empty, "Executive seats must be less than economy seats.");
            }

            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            await ProcessAirplaneCreationAsync(viewModel);


            TempData["SuccessMessage"] = "Airplane created successfully.";
            return RedirectToAction(nameof(Index));
        }



        /// <summary>
        /// Displays the form to edit an existing airplane.
        /// </summary>
        /// <param name="id">The ID of the airplane to edit.</param>
        /// <returns>
        /// Task: The edit airplane view, or a 404 error if not found.
        /// </returns>
        // GET: AirplanesController/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var airplane = await _airplaneRepository.GetByIdAsync(id);
            if (airplane == null)
                return new NotFoundViewResult("Error404");

            var model = _converterHelper.ToAirplaneViewModel(airplane);

            ViewBag.LockCapacity = _airplaneRepository.HasAnyNonCanceledFlights(id);
            ViewBag.LockStatus = false;

            return View(model);
        }


        /// <summary>
        /// Handles the submission of the airplane edit form.
        /// Updates airplane details, handles image changes, capacity changes, and status changes.
        /// </summary>
        /// <param name="viewModel">The updated airplane data from the form.</param>
        /// <returns>
        /// Task: Redirects to the Index on success, or returns the view with validation errors.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AirplaneViewModel viewModel)
        {
            // Initial validation for model state and seat capacity.
            if (!ModelState.IsValid || !ValidateSeatCapacity(viewModel))
                return View(viewModel);

            var airplane = await _airplaneRepository.GetByIdAsync(viewModel.Id);
            if (airplane == null) return new NotFoundViewResult("Error404");

            // Handle image update.
            Guid imageId = await HandleImageUpdateAsync(viewModel, airplane);

            // Check if capacity has changed and if there are existing flights.
            if (HasCapacityChangedWithFlights(viewModel, airplane))
            {
                ModelState.AddModelError("", "Cannot change seat capacity. This airplane has been used in flights.");
                return View(viewModel);
            }

            // Handle status change and potential conflicts.
            if (viewModel.Status != airplane.Status)
            {
                var statusChangeResult = await HandleStatusChangeAsync(viewModel, airplane);
                if (statusChangeResult != null) return statusChangeResult;
            }

            // Update airplane data and seats if capacity changed.
            await UpdateAirplaneAndSeatsAsync(viewModel, airplane, imageId);

            TempData["SuccessMessage"] = "Airplane updated successfully.";
            return RedirectToAction(nameof(Index));
        }



        /// <summary>
        /// Handles the deletion or deactivation of an airplane.
        /// Deletes if no associated flights, otherwise sets status to Inactive.
        /// </summary>
        /// <param name="id">The ID of the airplane to delete.</param>
        /// <returns>
        /// Task: Redirects to the Index with a success, info, or error message.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)

        {
            var airplane = await _airplaneRepository.GetByIdAsync(id);
            if (airplane == null)
            {
                return new NotFoundViewResult("Error404");
            }

            if (!_airplaneRepository.CanBeDeleted(id))
            {
                TempData["ErrorMessage"] = "This airplane has future scheduled flights and cannot be deleted or deactivated.";
                return RedirectToAction(nameof(Index));
            }

            await PerformAirplaneDeletionOrDeactivationAsync(airplane);

            return RedirectToAction(nameof(Index));
        }



        /// <summary>
        /// Processes the creation of an airplane, including image upload and seat generation.
        /// </summary>
        /// <param name="viewModel">The airplane data.</param>
        /// <returns>Task: A Task representing the asynchronous operation.</returns>
        private async Task ProcessAirplaneCreationAsync(AirplaneViewModel viewModel)
        {
            Guid imageId = Guid.Empty;
            if (viewModel.ImageFile != null && viewModel.ImageFile.Length > 0)
            {
                imageId = await _blobHelper.UploadBlobAsync(viewModel.ImageFile, "images");
            }

            var airplane = _converterHelper.ToAirplane(viewModel, imageId, true);
            await _airplaneRepository.CreateAsync(airplane);

            var seats = _seatGeneratorHelper.GenerateSeats(
                airplane.Id,
                airplane.TotalExecutiveSeats,
                airplane.TotalEconomySeats);
            await _airplaneRepository.AddSeatsAsync(seats);
        }


        /// <summary>
        /// Validates that executive seats are less than economy seats.
        /// </summary>
        /// <param name="viewModel">The airplane view model.</param>
        /// <returns>True if valid, false otherwise (adds error to ModelState).</returns>
        private bool ValidateSeatCapacity(AirplaneViewModel viewModel)
        {
            if (viewModel.TotalExecutiveSeats >= viewModel.TotalEconomySeats)
            {
                ModelState.AddModelError(string.Empty, "Executive seats must be less than economy seats.");
                return false;
            }
            return true;
        }


        /// <summary>
        /// Handles the upload of a new image file for the airplane.
        /// </summary>
        /// <param name="viewModel">The airplane view model containing the new image file.</param>
        /// <param name="airplane">The existing airplane entity.</param>
        /// <returns>Task: The new or existing image ID.</returns>
        private async Task<Guid> HandleImageUpdateAsync(AirplaneViewModel viewModel, Airplane airplane)
        {
            Guid imageId = airplane.ImageId;
            if (viewModel.ImageFile != null)
            {
                imageId = await _blobHelper.UploadBlobAsync(viewModel.ImageFile, "images");
            }
            return imageId;
        }


        /// <summary>
        /// Checks if the seat capacity has changed and if the airplane has non-canceled flights,
        /// which would prevent capacity changes.
        /// </summary>
        /// <param name="viewModel">The updated airplane view model.</param>
        /// <param name="airplane">The existing airplane entity.</param>
        /// <returns>True if capacity changed and there are flights, otherwise false.</returns>
        private bool HasCapacityChangedWithFlights(AirplaneViewModel viewModel, Airplane airplane)
        {
            var hasFlights = _airplaneRepository.HasAnyNonCanceledFlights(viewModel.Id);
            var hasCapacityChanged = viewModel.TotalExecutiveSeats != airplane.TotalExecutiveSeats
                                     || viewModel.TotalEconomySeats != airplane.TotalEconomySeats;
            return hasFlights && hasCapacityChanged;
        }



        /// <summary>
        /// Updates the airplane details and replaces seats if capacity has changed.
        /// </summary>
        /// <param name="viewModel">The updated airplane data.</param>
        /// <param name="airplane">The existing airplane entity.</param>
        /// <param name="imageId">The ID of the airplane's image.</param>
        /// <returns>Task: A Task representing the asynchronous operation.</returns>
        private async Task UpdateAirplaneAndSeatsAsync(AirplaneViewModel viewModel, Airplane airplane, Guid imageId)
        {
            // Determine if capacity has actually changed before potentially replacing seats
            bool capacityChanged = viewModel.TotalExecutiveSeats != airplane.TotalExecutiveSeats ||
                                   viewModel.TotalEconomySeats != airplane.TotalEconomySeats;

            airplane = _converterHelper.ToAirplane(viewModel, imageId, isNew: false);
            await _airplaneRepository.UpdateAsync(airplane);

            if (capacityChanged)
            {
                var newSeats = _seatGeneratorHelper.GenerateSeats(
                    airplane.Id,
                    airplane.TotalExecutiveSeats,
                    airplane.TotalEconomySeats);
                await _airplaneRepository.ReplaceSeatsAsync(airplane.Id, newSeats);
            }
        }



        /// <summary>
        /// Performs deletion or deactivation of an airplane based on its flight history.
        /// </summary>
        /// <param name="airplane">The airplane entity to process.</param>
        /// <returns>Task: A Task representing the asynchronous operation.</returns>
        private async Task PerformAirplaneDeletionOrDeactivationAsync(Airplane airplane)
        {
            if (_airplaneRepository.HasAnyNonCanceledFlights(airplane.Id))
            {
                airplane.Status = AirplaneStatus.Inactive;
                await _airplaneRepository.UpdateAsync(airplane);
                TempData["InfoMessage"] = "Airplane marked as Inactive.";
            }
            else
            {
                await _airplaneRepository.DeleteAsync(airplane);
                TempData["SuccessMessage"] = "Airplane deleted successfully.";
            }
        }


        /// <summary>
        /// Handles changes in an airplane's status, particularly for 'Maintenance' and 'Inactive'.
        /// Checks for existing flights and notifies employees if necessary.
        /// </summary>
        /// <param name="viewModel">The airplane view model with the new status.</param>
        /// <param name="airplane">The current airplane entity.</param>
        /// <returns>
        /// Task: Returns an IActionResult if an error or conflict prevents status change (e.g., View with errors), otherwise returns null.
        /// </returns>
        private async Task<IActionResult?> HandleStatusChangeAsync(AirplaneViewModel viewModel, Airplane airplane)
        {
            if (viewModel.Status == AirplaneStatus.Maintenance)
            {
                var affectedWithTickets = await _flightRepository.GetFutureFlightsWithSoldTicketsAsync(airplane.Id);

                if (affectedWithTickets.Any())
                {
                    await NotifyEmployeesByEmailAsync(airplane, affectedWithTickets, urgent: true);
                    await _notificationService.NotifyEmployeesAsync($"Airplane {airplane.Model} cannot enter maintenance yet: active flights with tickets.");
                    ModelState.AddModelError("", "This airplane is assigned to future flights with sold tickets. Please reassign them before changing the status to Maintenance.");
                    return View(viewModel);
                }

                var futureFlights = await _flightRepository.GetFutureFlightsByAirplaneAsync(airplane.Id);
                if (futureFlights.Any())
                {
                    await NotifyEmployeesByEmailAsync(airplane, futureFlights, urgent: false);
                    await _notificationService.NotifyEmployeesAsync($"Airplane {airplane.Model} cannot enter maintenance: assigned to scheduled flights.");
                    ModelState.AddModelError("", "This airplane is assigned to future flights. Please reassign them before changing the status to Maintenance.");
                    return View(viewModel);
                }

            }
            else if (viewModel.Status == AirplaneStatus.Inactive && _airplaneRepository.HasFutureScheduledFlights(viewModel.Id))
            {
                ModelState.AddModelError("", "Cannot inactivate airplane while it's assigned to future flights.");
                return View(viewModel);
            }

            return null;
        }



        /// <summary>
        /// Sends email notifications to employees regarding airplane status changes and affected flights.
        /// </summary>
        /// <param name="airplane">The airplane whose status has changed.</param>
        /// <param name="flights">A collection of affected flights.</param>
        /// <param name="urgent">Indicates if the notification is urgent (e.g., flights with sold tickets).</param>
        /// <returns>
        /// Task: A Task representing the asynchronous operation.
        /// </returns>
        private async Task NotifyEmployeesByEmailAsync(Airplane airplane, IEnumerable<Flight> flights, bool urgent)
        {
            var flightList = string.Join("<br/>", flights.Select(f =>
                $"- Flight {f.FlightNumber} (ID: {f.Id}) on {f.DepartureUtc:yyyy-MM-dd HH:mm}"));

            var subject = urgent
                ? $"URGENT: Airplane {airplane.Model} Requires Maintenance"
                : $"Heads-up: Airplane {airplane.Model} set to Maintenance";

            var intro = urgent
                ? "<p>The airplane has been marked for <strong>Maintenance</strong> but is still assigned to these future flights with sold tickets:</p>"
                : "<p>The airplane has been set to <strong>Maintenance</strong> and is assigned to the following flights (no tickets sold yet):</p>";

            var employees = await _userHelper.GetUsersInRoleAsync(UserRoles.Employee);

            foreach (var employee in employees)
            {
                var body = $@"
            <p>Hello {employee.FullName},</p>
            <p>The airplane <strong>{airplane.Model}</strong> (ID: {airplane.Id})</p>
            {intro}
            <p>{flightList}</p>
            <p>Please take the appropriate actions.</p>
            <p>Thank you,<br/>Operations Team</p>";

                await _mailHelper.SendEmailAsync(employee.Email, subject, body);
            }
        }

    }
}
