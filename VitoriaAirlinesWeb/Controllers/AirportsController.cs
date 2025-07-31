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
        /// <summary>
        /// Manages airport-related operations. Only accessible by users with the Admin role.
        /// </summary>
        private readonly IAirportRepository _airportRepository;
        private readonly IConverterHelper _converterHelper;
        private readonly ICountryRepository _countryRepository;


        /// <summary>
        /// Initializes a new instance of the AirportsController with necessary repositories and helpers.
        /// </summary>
        /// <param name="airportRepository">Repository for airport data access.</param>
        /// <param name="converterHelper">Helper for converting between entities and view models.</param>
        /// <param name="countryRepository">Repository for country data access.</param>
        public AirportsController(
            IAirportRepository airportRepository,
            IConverterHelper converterHelper,
            ICountryRepository countryRepository)
        {
            _airportRepository = airportRepository;
            _converterHelper = converterHelper;
            _countryRepository = countryRepository;
        }


        /// <summary>
        /// Displays a list of all airports, including their associated countries.
        /// </summary>
        /// <returns>
        /// IActionResult: A view displaying a collection of airports.
        /// </returns>
        // GET: AirportsController
        public IActionResult Index()
        {
            return View(_airportRepository.GetAllWithCountries());
        }


        /// <summary>
        /// Displays the details of a specific airport, including its country.
        /// </summary>
        /// <param name="id">The ID of the airport.</param>
        /// <returns>
        /// Task: A view displaying the airport details, or a 404 error if not found.
        /// </returns>
        // GET: AirportsController/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var airport = await _airportRepository.GetByIdWithCountryAsync(id);
            if (airport == null) return new NotFoundViewResult("Error404");

            return View(airport);
        }


        /// <summary>
        /// Displays the form to create a new airport.
        /// </summary>
        /// <returns>
        /// IActionResult: The create airport view, pre-populated with country options.
        /// </returns>
        // GET: AirportsController/Create
        public IActionResult Create()
        {
            var model = new AirportViewModel
            {
                Countries = _countryRepository.GetComboCountries()
            };

            return View(model);
        }



        /// <summary>
        /// Handles the submission of the new airport creation form.
        /// Validates input, checks for duplicate IATA codes, and creates the airport.
        /// </summary>
        /// <param name="viewModel">The airport data from the form.</param>
        /// <returns>
        /// Task: Redirects to the Index on success, or returns the view with validation errors and country options.
        /// </returns>
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


        /// <summary>
        /// Displays the form to edit an existing airport.
        /// </summary>
        /// <param name="id">The ID of the airport to edit.</param>
        /// <returns>
        /// Task: The edit airport view, pre-populated with airport data and country options, or a 404 error if not found.
        /// </returns>

        // GET: AirportsController/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var airplane = await _airportRepository.GetByIdWithCountryAsync(id);
            if (airplane == null) return new NotFoundViewResult("Error404");

            var model = _converterHelper.ToAirportViewModel(airplane);
            model.Countries = _countryRepository.GetComboCountries();
            return View(model);
        }



        /// <summary>
        /// Handles the submission of the airport edit form.
        /// Validates input, checks for associated flights, and updates the airport.
        /// </summary>
        /// <param name="id">The ID of the airport being edited.</param>
        /// <param name="viewModel">The updated airport data from the form.</param>
        /// <returns>
        /// Task: Redirects to the Index on success, or returns the view with validation/error messages and country options.
        /// </returns>
        // POST: AirportsController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AirportViewModel viewModel)
        {
            if (id != viewModel.Id) return new NotFoundViewResult("Error404");

            if (await _airportRepository.HasAssociatedFlightsAsync(id))
            {
                TempData["ErrorMessage"] = "Cannot update airport because it is associated with existing flights.";
                return RedirectToAction(nameof(Index));
            }

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


        /// <summary>
        /// Handles the deletion of an airport.
        /// Checks for associated flights before proceeding with deletion.
        /// </summary>
        /// <param name="id">The ID of the airport to delete.</param>
        /// <returns>
        /// Task: Redirects to the Index with a success or error message.
        /// </returns>
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

            TempData["SuccessMessage"] = "Airport deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
