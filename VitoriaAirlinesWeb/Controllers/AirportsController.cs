using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VitoriaAirlinesWeb.Data.Repositories;
using VitoriaAirlinesWeb.Helpers;
using VitoriaAirlinesWeb.Models.Airports;

namespace VitoriaAirlinesWeb.Controllers
{
    [Authorize(Roles = UserRoles.Admin)]
    public class AirportsController : Controller
    {
        private readonly IAirportRepository _airportRepository;
        private readonly IConverterHelper _converterHelper;
        private readonly ICountryRepository _countryRepository;

        public AirportsController(
            IAirportRepository airportRepository,
            IConverterHelper converterHelper,
            ICountryRepository countryRepository)
        {
            _airportRepository = airportRepository;
            _converterHelper = converterHelper;
            _countryRepository = countryRepository;
        }


        // GET: AirportsController
        public IActionResult Index()
        {
            return View(_airportRepository.GetAllWithCountries());
        }

        // GET: AirportsController/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var airport = await _airportRepository.GetByIdWithCountryAsync(id);
            if (airport == null) return new NotFoundViewResult("Error404");

            return View(airport);
        }

        // GET: AirportsController/Create
        public IActionResult Create()
        {
            var model = new AirportViewModel
            {
                Countries = _countryRepository.GetComboCountries()
            };

            return View(model);
        }

        // POST: AirportsController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AirportViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                viewModel.Countries = _countryRepository.GetComboCountries();
                return View(viewModel);
            }

            var existingAirport = _airportRepository.GetAll()
                .Any(a => a.IATA == viewModel.IATA.ToUpper());

            if (existingAirport)
            {
                ModelState.AddModelError(nameof(viewModel.IATA), "An airport with this IATA code already exists.");
                viewModel.Countries = _countryRepository.GetComboCountries();
                return View(viewModel);
            }

            var airport = _converterHelper.ToAirport(viewModel, isNew: true);
            await _airportRepository.CreateAsync(airport);

            TempData["SuccessMessage"] = "Airport created successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET: AirportsController/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var airplane = await _airportRepository.GetByIdWithCountryAsync(id);
            if (airplane == null) return new NotFoundViewResult("Error404");

            var model = _converterHelper.ToAirportViewModel(airplane);
            model.Countries = _countryRepository.GetComboCountries();
            return View(model);
        }

        // POST: AirportsController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AirportViewModel viewModel)
        {
            if (id != viewModel.Id) return new NotFoundViewResult("Error404");

            if (!ModelState.IsValid)
            {
                viewModel.Countries = _countryRepository.GetComboCountries();
                return View(viewModel);
            }

            var duplicateIata = _airportRepository.GetAll()
                                .Any(a => a.IATA == viewModel.IATA.ToUpper() && a.Id != viewModel.Id);

            if (duplicateIata)
            {
                ModelState.AddModelError(nameof(viewModel.IATA), "An airport with this IATA code already exists.");
                viewModel.Countries = _countryRepository.GetComboCountries();
                return View(viewModel);
            }

            var airport = _converterHelper.ToAirport(viewModel, isNew: false);
            await _airportRepository.UpdateAsync(airport);

            TempData["SuccessMessage"] = "Airport updated successfully.";
            return RedirectToAction(nameof(Index));
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var airport = await _airportRepository.GetByIdAsync(id);
            if (airport == null) return new NotFoundViewResult("Error404");

            if (await _airportRepository.HasAssociatedFlightsAsync(id))
            {
                TempData["ErrorMessage"] = "Cannot delete airport because it is associated with existing flights.";
                return RedirectToAction(nameof(Index));
            }

            await _airportRepository.DeleteAsync(airport);

            TempData["SuccessMessage"] = "Airplane deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
