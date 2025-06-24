using Microsoft.AspNetCore.Mvc;

namespace VitoriaAirlinesWeb.Controllers
{
    public class TicketsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
