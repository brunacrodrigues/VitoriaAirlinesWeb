using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VitoriaAirlinesWeb.Data.Repositories;
using VitoriaAirlinesWeb.Helpers;

namespace VitoriaAirlinesWeb.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly IUserHelper _userHelper;
        private readonly IFlightRepository _flightRepository;

        public DashboardController(
            IUserHelper userHelper,
            IFlightRepository flightRepository)
        {
            _userHelper = userHelper;
            _flightRepository = flightRepository;
        }

        public async Task<IActionResult> Index()
        {
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _userHelper.GetUserByEmailAsync(User.Identity.Name);
            if (user == null)
            {
                return NotFound();
            }

            if (User.IsInRole(UserRoles.Admin) || User.IsInRole(UserRoles.Employee))
            {
                ViewData["Role"] = User.IsInRole(UserRoles.Admin) ? "Admin" : "Employee";
                var flights = await _flightRepository.GetScheduledFlightsAsync();
                return View("Index", flights);
            }
            else if (User.IsInRole(UserRoles.Customer))
            {
                ViewData["Role"] = "Customer";
                return View("Index");
            }

            return RedirectToAction("Login", "Account");

        }
    }
}
