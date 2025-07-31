using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VitoriaAirlinesWeb.Data.Repositories;
using VitoriaAirlinesWeb.Helpers;

namespace VitoriaAirlinesWeb.Controllers
{
    /// <summary>
    /// Manages flight-related functionalities for authenticated customers, such as viewing flight history and upcoming flights, and canceling tickets.
    /// Only accessible by users with the Customer role.
    /// </summary>
    [Authorize(Roles = UserRoles.Customer)]
    public class MyFlightsController : Controller
    {
        private readonly IUserHelper _userHelper;
        private readonly ITicketRepository _ticketRepository;
        private readonly IMailHelper _mailHelper;
        private readonly IConverterHelper _converterHelper;


        /// <summary>
        /// Initializes a new instance of the MyFlightsController with necessary helpers and repositories.
        /// </summary>
        /// <param name="userHelper">Helper for user-related operations.</param>
        /// <param name="ticketRepository">Repository for ticket data access.</param>
        /// <param name="mailHelper">Helper for sending emails.</param>
        /// <param name="converterHelper">Helper for converting entities to view models.</param>
        public MyFlightsController(
            IUserHelper userHelper,
            ITicketRepository ticketRepository,
            IMailHelper mailHelper,
            IConverterHelper converterHelper)
        {
            _userHelper = userHelper;
            _ticketRepository = ticketRepository;
            _mailHelper = mailHelper;
            _converterHelper = converterHelper;
        }


        /// <summary>
        /// Displays the history of past flights for the current authenticated customer.
        /// </summary>
        /// <returns>
        /// Task: A view displaying a list of past tickets, or a 404 error if the user is not found.
        /// </returns>
        [HttpGet]
        public async Task<IActionResult> History()
        {
            var email = User.Identity?.Name;
            var user = await _userHelper.GetUserByEmailAsync(email);
            if (user == null) return new NotFoundViewResult("Error404");

            var tickets = await _ticketRepository.GetTicketsHistoryByUserAsync(user.Id);

            var model = _converterHelper.ToTicketDisplayViewModel(tickets);

            return View(model);
        }


        /// <summary>
        /// Displays the upcoming flights for the current authenticated customer.
        /// </summary>
        /// <returns>
        /// Task: A view displaying a list of upcoming tickets, or a 404 error if the user is not found.
        /// </returns>
        [HttpGet]
        public async Task<IActionResult> Upcoming()
        {
            var email = User.Identity?.Name;
            var user = await _userHelper.GetUserByEmailAsync(email);
            if (user == null) return new NotFoundViewResult("Error404");

            var tickets = await _ticketRepository.GetUpcomingTicketsByUserAsync(user.Id);

            return View(tickets);
        }


        /// <summary>
        /// Handles the cancellation of a customer's ticket.
        /// A ticket can only be canceled if it's not already canceled and is more than 24 hours before departure.
        /// Sends a confirmation email upon successful cancellation.
        /// </summary>
        /// <param name="id">The ID of the ticket to cancel.</param>
        /// <returns>
        /// Task: Redirects to the Upcoming flights page with a success or error message.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelTicket(int id)
        {
            var user = await _userHelper.GetUserByEmailAsync(User.Identity.Name);
            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction("Upcoming");
            }

            var ticket = await _ticketRepository.GetTicketWithDetailsAsync(id);
            if (ticket == null || ticket.IsCanceled || ticket.UserId != user.Id)
            {
                TempData["Error"] = "Invalid or already canceled ticket.";
                return RedirectToAction("Upcoming");
            }


            if (ticket.Flight.DepartureUtc <= DateTime.UtcNow.AddHours(24))
            {
                TempData["Error"] = "Tickets can only be canceled up to 24 hours before departure.";
                return RedirectToAction(nameof(Upcoming));

            }

            ticket.IsCanceled = true;
            ticket.CanceledDateUtc = DateTime.UtcNow;
            await _ticketRepository.UpdateAsync(ticket);

            var body = $@"<p>Hello {user.FullName},</p>
                        <p>Your ticket for flight <strong>{ticket.Flight.FlightNumber}</strong> scheduled on 
                        <strong>{TimezoneHelper.ConvertToLocal(ticket.Flight.DepartureUtc):dd/MM/yyyy HH:mm}</strong>
                        has been successfully canceled.</p>
                        <p>A refund will be issued to the email used during payment.</p>
                        <p>If you have any questions, feel free to contact us.</p>
                        <p>Thank you,<br/>Vitoria Airlines</p>";

            var emailResult = await _mailHelper.SendEmailAsync(user.Email, "Ticket Cancellation Confirmation", body);

            if (!emailResult.IsSuccess)
            {
                TempData["Error"] = "Ticket canceled, but failed to send confirmation email.";
            }
            else
            {
                TempData["Success"] = "Ticket successfully canceled. You will be issued with a refund.";
            }


            return RedirectToAction("Upcoming");

        }

    }
}
