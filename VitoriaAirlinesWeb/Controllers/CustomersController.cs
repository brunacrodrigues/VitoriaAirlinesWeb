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
        private readonly IBlobHelper _blobHelper;

        public CustomersController(
            IUserHelper userHelper,
            ICustomerProfileRepository customerRepository,
            IConverterHelper converterHelper,
            ICountryRepository countryRepository,
            IBlobHelper blobHelper)
        {
            _userHelper = userHelper;
            _customerRepository = customerRepository;
            _converterHelper = converterHelper;
            _countryRepository = countryRepository;
            _blobHelper = blobHelper;
        }


        // GET: CustomersController
        [Authorize(Roles = UserRoles.Admin)]
        public IActionResult Index()
        {
            var customers = _customerRepository.GetAll()
                .Include(c => c.User)
                .Include(c => c.Country)
                .OrderBy(c => c.User.FirstName)
                .ThenBy(c => c.User.LastName)
                .Select(c => new CustomerViewModel
                {
                    Id = c.Id,
                    FullName = c.User.FullName,
                    Email = c.User.Email,
                    PassportNumber = c.PassportNumber,
                    CountryName = c.Country != null ? c.Country.Name : null,
                    CountryFlagUrl = c.Country != null ? c.Country.FlagImageUrl : null
                })
                .ToList();

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

            var viewModel = new CustomerDetailsViewModel
            {
                Customer = customer,
                TotalFlights = customer.User.Tickets?.Count() ?? 0,
                LastFlight = lastFlight,
                NextFlight = nextFlight
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

            if (!ModelState.IsValid)
            {
                model.Countries = _countryRepository.GetComboCountries();
                return View(model);
            }

            var customer = await _customerRepository.GetByIdWithUserAsync(id);
            if (customer == null) return new NotFoundViewResult("Error404");

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

            if (!ModelState.IsValid)
            {
                model.Countries = _countryRepository.GetComboCountries();
                model.CurrentProfileImagePath = user.ImageFullPath;
                return View(model);
            }

            // Atualiza dados do utilizador
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;

            if (model.ImageFile != null && model.ImageFile.Length > 0)
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

            // Atualiza dados do perfil de cliente
            profile.CountryId = model.CountryId;
            profile.PassportNumber = model.PassportNumber;
            await _customerRepository.UpdateAsync(profile);

            TempData["SuccessMessage"] = "Profile updated successfully!";
            return RedirectToAction(nameof(EditTravellerProfile));
        }


        // POST: CustomersController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> Delete(int id)
        {
            var customer = await _customerRepository.GetByIdWithUserAsync(id);
            if (customer == null) return new NotFoundViewResult("Error404");

            try
            {
                var user = customer.User;

                if (user != null)
                {
                    await _userHelper.DeactivateUserAsync(user);

                    await _userHelper.RemoveUserFromRole(user, UserRoles.Customer);

                    await _userHelper.CheckRoleAsync(UserRoles.Deactivated);
                    await _userHelper.AddUserToRoleAsync(user, UserRoles.Deactivated);
                }

                await _customerRepository.DeleteAsync(customer);



                TempData["SuccessMessage"] = "Customer profile deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {

                if (ex.InnerException != null && ex.InnerException.Message.Contains("DELETE"))
                {
                    ViewBag.ErrorTitle = $"{customer.User.FullName} cannot be deleted";
                    ViewBag.ErrorMessage = $"{customer.User.FullName} cannot be deleted because has associated records.</br></br>" +
                        $"To proceed, you must first reassign or delete all related records that depend on this customer profile." +
                        $"Once all dependencies are removed, you may try deactivating the account again.";

                }

                return View("Error");
            }
        }

    }
}
