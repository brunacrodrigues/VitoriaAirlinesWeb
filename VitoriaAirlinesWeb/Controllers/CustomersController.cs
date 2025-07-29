using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VitoriaAirlinesWeb.Data.Repositories;
using VitoriaAirlinesWeb.Helpers;
using VitoriaAirlinesWeb.Models.ViewModels.Customers;

namespace VitoriaAirlinesWeb.Controllers
{
    [Authorize]
    public class CustomersController : Controller
    {
        private readonly IUserHelper _userHelper;
        private readonly ICustomerProfileRepository _customerRepository;
        private readonly IConverterHelper _converterHelper;
        private readonly ICountryRepository _countryRepository;
        private readonly ITicketRepository _ticketRepository;
        private readonly IBlobHelper _blobHelper;

        public CustomersController(
            IUserHelper userHelper,
            ICustomerProfileRepository customerRepository,
            IConverterHelper converterHelper,
            ICountryRepository countryRepository,
            ITicketRepository ticketRepository,
            IBlobHelper blobHelper)
        {
            _userHelper = userHelper;
            _customerRepository = customerRepository;
            _converterHelper = converterHelper;
            _countryRepository = countryRepository;
            _ticketRepository = ticketRepository;
            _blobHelper = blobHelper;
        }


        // GET: CustomersController
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> Index()
        {
            var customerEntities = _customerRepository.GetAll()
                .Include(c => c.User)
                .Include(c => c.Country)
                .OrderBy(c => c.User.FirstName)
                .ThenBy(c => c.User.LastName)
                .ToList();

            var customers = new List<CustomerViewModel>();

            foreach (var customer in customerEntities)
            {
                var roles = await _userHelper.GetUserRolesAsync(customer.User);
                var mainRole = roles.FirstOrDefault(); // ex: Customer, Deactivated

                customers.Add(new CustomerViewModel
                {
                    Id = customer.Id,
                    FullName = customer.User.FullName,
                    Email = customer.User.Email,
                    PassportNumber = customer.PassportNumber,
                    CountryName = customer.Country?.Name,
                    CountryFlagUrl = customer.Country?.FlagImageUrl,
                    Role = mainRole
                });
            }

            return View(customers);

        }

        // GET: CustomersController/Details/5
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> Details(int id)
        {
            var customer = await _customerRepository.GetProfileWithUserAndFlightsAsync(id);

            if (customer == null) return new NotFoundViewResult("Error404");

            var now = DateTime.UtcNow;

            var flights = customer.User.Tickets?
                .Where(t => t.Flight != null)
                .Select(t => t.Flight!)
                .ToList();

            var lastFlight = flights
                .Where(f => f.DepartureUtc < now)
                .OrderByDescending(f => f.DepartureUtc)
                .FirstOrDefault();

            var nextFlight = flights
                .Where(f => f.DepartureUtc >= now)
                .OrderBy(f => f.DepartureUtc)
                .FirstOrDefault();

            var roles = await _userHelper.GetUserRolesAsync(customer.User);
            var role = roles.FirstOrDefault();

            var viewModel = new CustomerDetailsViewModel
            {
                Customer = customer,
                TotalFlights = customer.User.Tickets?.Count() ?? 0,
                LastFlight = lastFlight,
                NextFlight = nextFlight,
                Role = role
            };


            return View(viewModel);
        }



        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> EditCustomerProfile(int id)
        {
            var customer = await _customerRepository.GetByIdWithUserAsync(id);
            if (customer == null) return new NotFoundViewResult("Error404");

            var model = _converterHelper.ToCustomerProfileAdminViewModel(customer);
            model.Countries = _countryRepository.GetComboCountries();
            return View(model);
        }


        [HttpPost]
        [Authorize(Roles = UserRoles.Admin)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCustomerProfile(int id, CustomerProfileAdminViewModel model)
        {
            if (id != model.Id) return new NotFoundViewResult("Error404");

            var customer = await _customerRepository.GetByIdWithUserAsync(id);
            if (customer == null) return new NotFoundViewResult("Error404");

            if (model.CountryId == 0)
            {
                model.CountryId = null;
            }


            if (string.IsNullOrWhiteSpace(model.PassportNumber) && !string.IsNullOrWhiteSpace(customer.PassportNumber))
            {
                ModelState.AddModelError(nameof(model.PassportNumber), "You cannot remove the customer's passport number once it's set.");
            }


            if (!string.IsNullOrWhiteSpace(model.PassportNumber))
            {
                var existing = await _customerRepository.GetByPassportAsync(model.PassportNumber);
                if (existing != null && existing.Id != model.Id)
                {
                    ModelState.AddModelError(nameof(model.PassportNumber), "This passport number is already associated with another customer.");
                }
            }

            if (!ModelState.IsValid)
            {
                model.Countries = _countryRepository.GetComboCountries();
                model.PassportNumber = customer.PassportNumber;
                model.FullName = customer.User.FullName;
                model.Email = customer.User.Email;

                ModelState.SetModelValue(nameof(model.PassportNumber), model.PassportNumber, model.PassportNumber);
                ModelState.SetModelValue(nameof(model.FullName), model.FullName, model.FullName);
                ModelState.SetModelValue(nameof(model.Email), model.Email, model.Email);

                return View(model);
            }

            _converterHelper.UpdateCustomerProfile(customer, model);
            await _customerRepository.UpdateAsync(customer);

            TempData["SuccessMessage"] = "Customer profile updated successfully.";
            return RedirectToAction(nameof(Index));
        }


        [Authorize(Roles = UserRoles.Customer)]
        public async Task<IActionResult> EditTravellerProfile()
        {
            var user = await _userHelper.GetUserByEmailAsync(User.Identity.Name);
            if (user == null) return new NotFoundViewResult("Error404"); ;

            var profile = await _customerRepository.GetByUserIdAsync(user.Id);
            if (profile == null) return new NotFoundViewResult("Error404");

            var model = _converterHelper.ToCustomerProfileViewModel(profile, user);
            model.Countries = _countryRepository.GetComboCountries();

            return View(model);
        }


        [HttpPost]
        [Authorize(Roles = UserRoles.Customer)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditTravellerProfile(CustomerProfileViewModel model)
        {
            var user = await _userHelper.GetUserByEmailAsync(User.Identity.Name);
            if (user == null) return new NotFoundViewResult("Error404");

            var profile = await _customerRepository.GetByUserIdAsync(user.Id);
            if (profile == null) return new NotFoundViewResult("Error404");


            if (model.CountryId == 0)
            {
                model.CountryId = null;
            }


            if (string.IsNullOrWhiteSpace(model.PassportNumber) && !string.IsNullOrWhiteSpace(profile.PassportNumber))
            {
                ModelState.AddModelError(nameof(model.PassportNumber), "You cannot remove your passport number once it's set.");
            }

            if (!string.IsNullOrWhiteSpace(model.PassportNumber))
            {
                var existing = await _customerRepository.GetByPassportAsync(model.PassportNumber);
                if (existing != null && existing.Id != profile.Id)
                {
                    ModelState.AddModelError(nameof(model.PassportNumber), "Invalid passport number.");
                }
            }

            if (!ModelState.IsValid)
            {

                model.Countries = _countryRepository.GetComboCountries();
                model.CurrentProfileImagePath = user.ImageFullPath;
                model.PassportNumber = profile.PassportNumber;

                ModelState.SetModelValue(nameof(model.PassportNumber), model.PassportNumber, model.PassportNumber);
                return View(model);
            }


            user.FirstName = model.FirstName;
            user.LastName = model.LastName;

            if (Request.Form["RemoveImage"] == "true")
            {
                user.ProfileImageId = null;
            }
            else if (model.ImageFile != null && model.ImageFile.Length > 0)
            {
                var imageId = await _blobHelper.UploadBlobAsync(model.ImageFile, "images");
                user.ProfileImageId = imageId;
            }

            var response = await _userHelper.UpdateUserAsync(user);
            if (!response.Succeeded)
            {
                ModelState.AddModelError(string.Empty, response.Errors.FirstOrDefault()?.Description ?? "An error occurred.");
                model.Countries = _countryRepository.GetComboCountries();
                model.CurrentProfileImagePath = user.ImageFullPath;
                return View(model);
            }


            profile.CountryId = model.CountryId;
            profile.PassportNumber = model.PassportNumber;
            await _customerRepository.UpdateAsync(profile);

            TempData["SuccessMessage"] = "Profile updated successfully!";
            return RedirectToAction(nameof(EditTravellerProfile));
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> Delete(int id)
        {
            var customer = await _customerRepository.GetByIdWithUserAsync(id);
            if (customer == null) return new NotFoundViewResult("Error404");

            var user = customer.User;
            if (user == null) return View("Error");


            var hasUpcomingFlights = await _ticketRepository.HasUpcomingFlightsAsync(user.Id);
            if (hasUpcomingFlights)
            {
                TempData["WarningMessage"] = "This customer has upcoming flights and cannot be deactivated at this time.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                await _userHelper.DeactivateUserAsync(user);
                await _userHelper.RemoveUserFromRole(user, UserRoles.Customer);

                await _userHelper.CheckRoleAsync(UserRoles.Deactivated);
                await _userHelper.AddUserToRoleAsync(user, UserRoles.Deactivated);

                TempData["SuccessMessage"] = "Customer account deactivated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ViewBag.ErrorTitle = $"{user.FullName} could not be deactivated.";
                ViewBag.ErrorMessage = $"An error occurred while trying to deactivate this account: {ex.Message}";
                return View("Error");
            }
        }

    }
}
