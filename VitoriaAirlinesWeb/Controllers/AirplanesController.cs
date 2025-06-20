using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VitoriaAirlinesWeb.Data.Enums;
using VitoriaAirlinesWeb.Data.Repositories;
using VitoriaAirlinesWeb.Helpers;
using VitoriaAirlinesWeb.Models.Airplane;

namespace VitoriaAirlinesWeb.Controllers
{
    [Authorize(Roles = UserRoles.Admin)]
    public class AirplanesController : Controller
    {
        private readonly IAirplaneRepository _airplaneRepository;
        private readonly ISeatGeneratorHelper _seatGeneratorHelper;
        private readonly IConverterHelper _converterHelper;
        private readonly IBlobHelper _blobHelper;

        public AirplanesController(
            IAirplaneRepository airplaneRepository,
            ISeatGeneratorHelper seatGeneratorHelper,
            IConverterHelper converterHelper,
            IBlobHelper blobHelper)
        {
            _airplaneRepository = airplaneRepository;
            _seatGeneratorHelper = seatGeneratorHelper;
            _converterHelper = converterHelper;
            _blobHelper = blobHelper;
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
                return NotFound();

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
                return NotFound();

            var model = _converterHelper.ToAirplaneViewModel(airplane);

            var hasFlights = _airplaneRepository.HasAnyNonCanceledFlights(id);
            ViewBag.LockCapacity = hasFlights;
            ViewBag.LockStatus = _airplaneRepository.HasFutureScheduledFlights(id);

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
                return NotFound();

            Guid imageId = airplane.ImageId;

            if (viewModel.ImageFile != null)
            {
                imageId = await _blobHelper.UploadBlobAsync(viewModel.ImageFile, "images");
            }

            // TODO para quando houver bilhetes vendidos 

            var hasFlights = _airplaneRepository.HasAnyNonCanceledFlights(viewModel.Id);
            var hasFutureFlights = _airplaneRepository.HasFutureScheduledFlights(viewModel.Id);

            if (hasFlights &&
                (viewModel.TotalExecutiveSeats != airplane.TotalExecutiveSeats ||
                 viewModel.TotalEconomySeats != airplane.TotalEconomySeats))
            {
                ModelState.AddModelError("", "Cannot change seat capacity. This airplane has been used in flights.");
                return View(viewModel);
            }

            if (hasFutureFlights && viewModel.Status != airplane.Status)
            {
                ModelState.AddModelError("", "Cannot change status. This airplane is assigned to future flights.");
                return View(viewModel);
            }

            airplane = _converterHelper.ToAirplane(viewModel, imageId, isNew: false);
            await _airplaneRepository.UpdateAsync(airplane);

            var newSeats = _seatGeneratorHelper.GenerateSeats(
                airplane.Id,
                airplane.TotalExecutiveSeats,
                airplane.TotalEconomySeats
            );
            await _airplaneRepository.ReplaceSeatsAsync(airplane.Id, newSeats);

            TempData["SuccessMessage"] = "Airplane updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET: AirplanesController/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var airplane = await _airplaneRepository.GetByIdAsync(id);
            if (airplane == null) return NotFound();

            if (!_airplaneRepository.CanBeDeleted(id))
            {
                TempData["ErrorMessage"] = "This airplane has future scheduled flights and cannot be deleted.";
                return RedirectToAction(nameof(Index));
            }

            if (_airplaneRepository.HasAnyNonCanceledFlights(id))
            {
                ViewBag.CanOnlyInactivate = true;
            }

            return View(airplane);
        }

        // POST: AirplanesController/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var airplane = await _airplaneRepository.GetByIdAsync(id);
            if (airplane == null)
            {
                return NotFound();
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
