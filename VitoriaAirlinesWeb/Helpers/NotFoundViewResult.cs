using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace VitoriaAirlinesWeb.Helpers
{
    /// <summary>
    /// Represents an ActionResult that renders a specific view with a 404 Not Found status code.
    /// </summary>
    public class NotFoundViewResult : ViewResult
    {
        /// <summary>
        /// Initializes a new instance of the NotFoundViewResult class.
        /// </summary>
        /// <param name="viewName">The name of the view to render (e.g., "Error404").</param>
        public NotFoundViewResult(string viewName)
        {
            ViewName = viewName; // Set the view to be rendered.
            StatusCode = (int)HttpStatusCode.NotFound; // Set the HTTP status code to 404.
        }
    }
}