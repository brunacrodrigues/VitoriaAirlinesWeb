using Microsoft.AspNetCore.Mvc;

namespace VitoriaAirlinesWeb.Controllers
{
    /// <summary>
    /// Handles custom error pages for the application.
    /// </summary>
    public class ErrorsController : Controller
    {
        /// <summary>
        /// Displays a custom 404 Not Found error page. This action is typically
        /// routed to when a requested resource is not found.
        /// </summary>
        /// <returns>
        /// IActionResult: The "Error404" view.
        /// </returns>
        [HttpGet("/Errors/Handle404")]
        public IActionResult Handle404()
        {
            return View("Error404");
        }
    }
}
