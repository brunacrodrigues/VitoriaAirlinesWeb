using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VitoriaAirlinesWeb.Data.Repositories;
using VitoriaAirlinesWeb.Models.Dtos;

namespace VitoriaAirlinesWeb.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]

    public class MyFlightsController : ControllerBase
    {
        private readonly ITicketRepository _ticketRepository;

        public MyFlightsController(ITicketRepository ticketRepository)
        {
            _ticketRepository = ticketRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetMyUpcomingFlights()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var tickets = await _ticketRepository.GetUpcomingTicketsByUserAsync(userId);

            var result = tickets.Select(t => new TicketDto
            {
                TicketId = t.Id,
                FlightNumber = t.Flight.FlightNumber,
                DepartureUtc = t.Flight.DepartureUtc,
                OriginAirport = t.Flight.OriginAirport.IATA,
                DestinationAirport = t.Flight.DestinationAirport.IATA,
                Seat = $"{t.Seat.Row}{t.Seat.Letter}",
                PricePaid = t.PricePaid,
                PurchaseDateUtc = t.PurchaseDateUtc
            });

            return Ok(result);
        }

    }
}
