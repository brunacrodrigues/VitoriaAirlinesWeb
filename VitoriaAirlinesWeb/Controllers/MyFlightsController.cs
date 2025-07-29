using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VitoriaAirlinesWeb.Data.Repositories;
using VitoriaAirlinesWeb.Helpers;

namespace VitoriaAirlinesWeb.Controllers
{
    [Authorize(Roles = UserRoles.Customer)]
    public class MyFlightsController : Controller
    {
        private readonly IUserHelper _userHelper;
        private readonly ITicketRepository _ticketRepository;
        private readonly IMailHelper _mailHelper;
        private readonly IConverterHelper _converterHelper;

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


        [HttpGet]
        public async Task<IActionResult> Upcoming()
        {
            var email = User.Identity?.Name;
            var user = await _userHelper.GetUserByEmailAsync(email);
            if (user == null) return new NotFoundViewResult("Error404");

            var tickets = await _ticketRepository.GetUpcomingTicketsByUserAsync(user.Id);

            return View(tickets);
        }


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
