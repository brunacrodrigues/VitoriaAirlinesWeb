using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VitoriaAirlinesWeb.Helpers;

namespace VitoriaAirlinesWeb.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly IUserHelper _userHelper;

        public DashboardController(IUserHelper userHelper)
        {
            _userHelper = userHelper;
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

            if (User.IsInRole(UserRoles.Admin))
            {
                ViewData["Role"] = "Admin";
            }
            else if (User.IsInRole(UserRoles.Employee))
            {
                ViewData["Role"] = "Employee";
            }
            else if (User.IsInRole(UserRoles.Customer))
            {
                ViewData["Role"] = "Customer";
            }

            ViewData["Title"] = "Dashboard";
            return View();
        }
    }
}
