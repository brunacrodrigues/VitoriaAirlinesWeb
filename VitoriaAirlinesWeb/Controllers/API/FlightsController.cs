using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VitoriaAirlinesWeb.Data.Repositories;
using VitoriaAirlinesWeb.Helpers;
using VitoriaAirlinesWeb.Models.Dtos;

namespace VitoriaAirlinesWeb.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]

    public class FlightsController : ControllerBase
    {
        private readonly ITicketRepository _ticketRepository;
        private readonly IUserHelper _userHelper;

        public FlightsController(ITicketRepository ticketRepository, IUserHelper userHelper)
        {
            _ticketRepository = ticketRepository;
            _userHelper = userHelper;
        }

        [HttpGet("{userId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetUserUpcomingFlights(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return BadRequest("UserId is required.");
            }

            var user = await _userHelper.GetUserByIdAsync(userId);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            var tickets = await _ticketRepository.GetUpcomingTicketsByUserAsync(userId);

            var result = tickets.Select(t => new TicketDto
            {
                TicketId = t.Id,
                FlightNumber = t.Flight.FlightNumber,
                DepartureUtc = t.Flight.DepartureUtc,
                OriginAirport = t.Flight.OriginAirport.IATA,
                DestinationAirport = t.Flight.DestinationAirport.IATA,
                Seat = $"{t.Seat.Row}{t.Seat.Letter} {t.Seat.Class}",
                PricePaid = t.PricePaid,
                PurchaseDateUtc = t.PurchaseDateUtc
            });

            return Ok(result);
        }


    }
}
