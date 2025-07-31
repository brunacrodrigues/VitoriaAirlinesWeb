using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VitoriaAirlinesWeb.Data.Repositories;
using VitoriaAirlinesWeb.Helpers;
using VitoriaAirlinesWeb.Models.Dtos;

namespace VitoriaAirlinesWeb.Controllers.API
{
    /// <summary>
    /// API controller responsible for retrieving flight-related information for users.
    /// Requires JWT authentication by default, except for explicitly allowed actions.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]

    public class FlightsController : ControllerBase
    {
        private readonly ITicketRepository _ticketRepository;
        private readonly IUserHelper _userHelper;


        /// <summary>
        /// Initializes a new instance of the <see cref="FlightsController"/> class.
        /// </summary>
        /// <param name="ticketRepository">Repository used for accessing ticket data.</param>
        /// <param name="userHelper">Helper service used for user-related operations.</param>
        public FlightsController(ITicketRepository ticketRepository, IUserHelper userHelper)
        {
            _ticketRepository = ticketRepository;
            _userHelper = userHelper;
        }


        /// <summary>
        /// Retrieves the list of upcoming flights for a given user based on their user ID.
        /// </summary>
        /// <param name="userId">The ID of the user whose upcoming flights are to be retrieved.</param>
        /// <returns>A list of upcoming flight tickets in a simplified DTO format, or an error message.</returns>
        [HttpGet("by-id/{userId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetUserUpcomingFlightsById(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return BadRequest("UserId is required.");

            var user = await _userHelper.GetUserByIdAsync(userId);
            if (user == null)
                return NotFound("User not found.");

            var tickets = await _ticketRepository.GetUpcomingTicketsByUserAsync(user.Id);

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


        /// <summary>
        /// Retrieves the list of upcoming flights for a given user based on their email.
        /// </summary>
        /// <param name="email">The email of the user whose upcoming flights are to be retrieved.</param>
        /// <returns>A list of upcoming flight tickets in a simplified DTO format, or an error message.</returns>
        [HttpGet("by-email/{email}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetUserUpcomingFlightsByEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return BadRequest("Email is required.");

            var user = await _userHelper.GetUserByEmailAsync(email);
            if (user == null)
                return NotFound("User not found.");

            var tickets = await _ticketRepository.GetUpcomingTicketsByUserAsync(user.Id);

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
