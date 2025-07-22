using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VitoriaAirlinesWeb.Data.Enums;
using VitoriaAirlinesWeb.Data.Repositories;
using VitoriaAirlinesWeb.Helpers;
using VitoriaAirlinesWeb.Models.ViewModels.Airplanes;
using VitoriaAirlinesWeb.Services;

namespace VitoriaAirlinesWeb.Controllers
{
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

        // GET: AirplanesController
        public IActionResult Index()
        {
            return View(_airplaneRepository.GetAll()
                .OrderBy(a => a.Model));
        }


        // GET: AirplanesController/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var airplane = await _airplaneRepository.GetByIdWithSeatsAsync(id);
            if (airplane == null)
                return new NotFoundViewResult("Error404");

            return View(airplane);
        }

        // GET: AirplanesController/Create
        public IActionResult Create()
        {
            return View();
        }

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

            TempData["SuccessMessage"] = "Airplane created successfully.";
            return RedirectToAction(nameof(Index));
        }


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

        // POST: AirplanesController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AirplaneViewModel viewModel)
        {
            if (!ModelState.IsValid)
                return View(viewModel);

            if (viewModel.TotalExecutiveSeats >= viewModel.TotalEconomySeats)
            {
                ModelState.AddModelError(string.Empty, "Executive seats must be less than economy seats.");
                return View(viewModel);
            }

            var airplane = await _airplaneRepository.GetByIdAsync(viewModel.Id);
            if (airplane == null)
                return new NotFoundViewResult("Error404");

            Guid imageId = airplane.ImageId;

            if (viewModel.ImageFile != null)
            {
                imageId = await _blobHelper.UploadBlobAsync(viewModel.ImageFile, "images");
            }


            var hasFlights = _airplaneRepository.HasAnyNonCanceledFlights(viewModel.Id);
            var hasFutureFlights = _airplaneRepository.HasFutureScheduledFlights(viewModel.Id);

            var hasCapacityChanged = viewModel.TotalExecutiveSeats != airplane.TotalExecutiveSeats
                       || viewModel.TotalEconomySeats != airplane.TotalEconomySeats;

            if (hasFlights &&
                (hasCapacityChanged))
            {
                ModelState.AddModelError("", "Cannot change seat capacity. This airplane has been used in flights.");
                return View(viewModel);
            }


            if (viewModel.Status != airplane.Status)
            {
                if (viewModel.Status == AirplaneStatus.Maintenance)
                {
                    var affectedFlights = await _flightRepository.GetFutureFlightsWithSoldTicketsAsync(airplane.Id);

                    if (affectedFlights.Any())
                    {

                        var flightList = string.Join(
                            "<br/>", affectedFlights.Select(f => $"- Flight {f.FlightNumber} (ID: {f.Id}) on {f.DepartureUtc:yyyy-MM-dd HH:mm}"));


                        var employees = await _userHelper.GetUsersInRoleAsync(UserRoles.Employee);

                        foreach (var employee in employees)
                        {
                            var subject = $"URGENT: Airplane {airplane.Model} Requires Maintenance";
                            var body = $@"
                                        <p>Hello {employee.FullName},</p>
                                        <p>The airplane <strong>{airplane.Model}</strong> (ID: {airplane.Id}) 
                                        has been marked for <strong>Maintenance</strong> but is still assigned to these future flights with sold tickets:</p>
                                        <p>{flightList}</p>
                                        <p>Please reassign those flights to another aircraft before taking this one offline for maintenance.</p>
                                        <p>Thank you,<br/>Operations Team</p>
                                    ";

                            await _mailHelper.SendEmailAsync(employee.Email, subject, body);

                        }

                        ModelState.AddModelError("", "This airplane is assigned to future flights with sold tickets. Please reassign them before changing the status to Maintenance.");
                        return View(viewModel);
                    }
                }
                else if (viewModel.Status == AirplaneStatus.Inactive && hasFutureFlights)
                {
                    ModelState.AddModelError("", "Cannot inactivate airplane while it's assigned to future flights.");
                    return View(viewModel);
                }
            }


            airplane = _converterHelper.ToAirplane(viewModel, imageId, isNew: false);
            await _airplaneRepository.UpdateAsync(airplane);

            if (hasCapacityChanged)
            {
                var newSeats = _seatGeneratorHelper.GenerateSeats(
                       airplane.Id,
                       airplane.TotalExecutiveSeats,
                       airplane.TotalEconomySeats
                   );
                await _airplaneRepository.ReplaceSeatsAsync(airplane.Id, newSeats);
            }

            TempData["SuccessMessage"] = "Airplane updated successfully.";
            return RedirectToAction(nameof(Index));
        }



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

            if (_airplaneRepository.HasAnyNonCanceledFlights(id))
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


            return RedirectToAction(nameof(Index));
        }
    }
}
