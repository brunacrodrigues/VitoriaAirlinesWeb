using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VitoriaAirlinesWeb.Data.Repositories;
using VitoriaAirlinesWeb.Helpers;
using VitoriaAirlinesWeb.Models.Customers;

namespace VitoriaAirlinesWeb.Controllers
{
    [Authorize]
    public class CustomersController : Controller
    {
        private readonly IUserHelper _userHelper;
        private readonly ICustomerProfileRepository _customerRepository;
        private readonly IConverterHelper _converterHelper;
        private readonly ICountryRepository _countryRepository;

        public CustomersController(
            IUserHelper userHelper,
            ICustomerProfileRepository customerRepository,
            IConverterHelper converterHelper,
            ICountryRepository countryRepository)
        {
            _userHelper = userHelper;
            _customerRepository = customerRepository;
            _converterHelper = converterHelper;
            _countryRepository = countryRepository;
        }


        // GET: CustomersController
        [Authorize(Roles = UserRoles.Admin)]
        public IActionResult Index()
        {
            return View(_customerRepository.GetAll()
                .Include(c => c.User)
                .Include(c => c.Country)
                .OrderBy(c => c.User.FirstName)
                .ThenBy(c => c.User.LastName));
        }

        // GET: CustomersController/Details/5
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> Details(int id)
        {

            var customer = await _customerRepository.GetByIdWithUserAsync(id);

            if (customer == null) return NotFound();

            return View(customer);
        }



        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> EditCustomerProfile(int id)
        {
            var customer = await _customerRepository.GetByIdWithUserAsync(id);
            if (customer == null) return NotFound();

            var model = _converterHelper.ToCustomerProfileAdminViewModel(customer);
            model.Countries = _countryRepository.GetComboCountries();
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = UserRoles.Admin)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCustomerProfile(int id, CustomerProfileAdminViewModel model)
        {

            if (id != model.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                model.Countries = _countryRepository.GetComboCountries();
                return View(model);
            }

            var customer = await _customerRepository.GetByIdWithUserAsync(id);
            if (customer == null) return NotFound();

            _converterHelper.UpdateCustomerProfile(customer, model);
            await _customerRepository.UpdateAsync(customer);

            TempData["SuccessMessage"] = "Customer profile updated successfully.";
            return RedirectToAction(nameof(Index));
        }


        [Authorize(Roles = UserRoles.Customer)]
        public async Task<IActionResult> EditTravellerProfile()
        {
            var user = await _userHelper.GetUserByEmailAsync(User.Identity.Name);
            if (user == null) return NotFound();

            var profile = await _customerRepository.GetByUserIdAsync(user.Id);
            if (profile == null) return NotFound();

            var model = _converterHelper.ToCustomerProfileViewModel(profile);
            model.Countries = _countryRepository.GetComboCountries();

            return View(model);
        }


        [HttpPost]
        [Authorize(Roles = UserRoles.Customer)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditTravellerProfile(CustomerProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Countries = _countryRepository.GetComboCountries();
                return View(model);
            }

            var user = await _userHelper.GetUserByEmailAsync(User.Identity.Name);
            if (user == null) return NotFound();

            var profile = await _customerRepository.GetByUserIdAsync(user.Id);
            if (profile == null) return NotFound();

            _converterHelper.UpdateCustomerProfile(profile, model);
            await _customerRepository.UpdateAsync(profile);

            TempData["SuccessMessage"] = "Your profile has been updated.";
            return RedirectToAction(nameof(EditTravellerProfile));
        }

        // GET: CustomersController/Delete/5
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> Delete(int id)
        {
            var customer = await _customerRepository.GetByIdWithUserAsync(id);
            if (customer == null) return NotFound();

            return View(customer);
        }


        // POST: CustomersController/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var customer = await _customerRepository.GetByIdWithUserAsync(id);
            if (customer == null) return NotFound();

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
