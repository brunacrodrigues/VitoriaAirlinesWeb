using Microsoft.AspNetCore.Mvc;

namespace VitoriaAirlinesWeb.Controllers
{
    public class ErrorsController : Controller
    {
        [HttpGet("/Errors/Handle404")]

        public IActionResult Handle404()
        {
            return View("Error404");
        }
    }
}
