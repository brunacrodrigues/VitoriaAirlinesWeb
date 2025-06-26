using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VitoriaAirlinesWeb.Data.Entities;
using VitoriaAirlinesWeb.Data.Repositories;
using VitoriaAirlinesWeb.Helpers;
using X.PagedList;
using X.PagedList.Extensions;

namespace VitoriaAirlinesWeb.Controllers
{
    [Authorize(Roles = UserRoles.Customer)]
    public class MyFlightsController : Controller
    {
        private readonly IUserHelper _userHelper;
        private readonly ITicketRepository _ticketRepository;

        public MyFlightsController(
            IUserHelper userHelper,
            ITicketRepository ticketRepository)
        {
            _userHelper = userHelper;
            _ticketRepository = ticketRepository;
        }

        [HttpGet]
        public async Task<IActionResult> History(int page = 1)
        {
            var email = User.Identity?.Name;
            var user = await _userHelper.GetUserByEmailAsync(email);
            if (user == null) return NotFound();

            var tickets = await _ticketRepository.GetTicketsHistoryByUserAsync(user.Id);

            const int pageSize = 10;
            IPagedList<Ticket> model = tickets.ToPagedList(page, pageSize);


            return View(model);
        }


        [HttpGet]
        public async Task<IActionResult> Upcoming(int page = 1)
        {
            var email = User.Identity?.Name;
            var user = await _userHelper.GetUserByEmailAsync(email);
            if (user == null) return NotFound();

            var tickets = await _ticketRepository.GetUpcomingTicketsByUserAsync(user.Id);

            const int pageSize = 10;
            IPagedList<Ticket> model = tickets.ToPagedList(page, pageSize);


            return View(model);
        }

    }
}
