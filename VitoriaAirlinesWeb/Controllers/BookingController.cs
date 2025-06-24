using Microsoft.AspNetCore.Mvc;
using VitoriaAirlinesWeb.Data.Enums;
using VitoriaAirlinesWeb.Data.Repositories;
using VitoriaAirlinesWeb.Helpers;

namespace VitoriaAirlinesWeb.Controllers
{
    public class BookingController : Controller
    {
        private readonly IFlightRepository _flightRepository;
        private readonly ITicketRepository _ticketRepository;
        private readonly IConverterHelper _converterHelper;

        public BookingController(
            IFlightRepository flightRepository,
            ITicketRepository ticketRepository,
            IConverterHelper converterHelper)
        {
            _flightRepository = flightRepository;
            _ticketRepository = ticketRepository;
            _converterHelper = converterHelper;
        }

        [HttpGet]
        public async Task<IActionResult> SelectSeat(int flightId)
        {
            if (User.Identity.IsAuthenticated && (User.IsInRole(UserRoles.Admin) || User.IsInRole(UserRoles.Employee)))
            {
                TempData["Error"] = "This functionality is reserved for customers only.";
                return RedirectToAction("Index", "Home");
            }


            var flight = await _flightRepository.GetByIdWithAirplaneAndSeatsAsync(flightId);
            if (flight == null || flight.Status != FlightStatus.Scheduled)
            {
                TempData["Error"] = "This flight is no longer available for booking.";
                return RedirectToAction("Index", "Home");
            }

            var occupiedSeatIds = (await _ticketRepository.GetTicketsByFlightAsync(flightId))
                .Select(t => t.SeatId);

            var viewModel = _converterHelper.ToSelectSeatViewModelAsync(flight, occupiedSeatIds);

            return View(viewModel);
        }
    }
}
