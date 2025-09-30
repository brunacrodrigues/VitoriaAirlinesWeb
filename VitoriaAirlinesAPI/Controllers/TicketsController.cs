using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VitoriaAirlinesAPI.Dtos;
using VitoriaAirlinesAPI.Services;
using VitoriaAirlinesWeb.Data.Repositories;
using VitoriaAirlinesWeb.Helpers;

namespace VitoriaAirlinesAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(
    AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
    Roles = UserRoles.Customer)]
    public class TicketsController : ControllerBase
    {
        private readonly ITicketRepository _ticketRepository;
        private readonly ITicketPdfService _ticketPdfService;

        public TicketsController(ITicketRepository ticketRepository, ITicketPdfService ticketPdfService)
        {
            _ticketRepository = ticketRepository;
            _ticketPdfService = ticketPdfService;
        }

        [HttpGet("{ticketId}/boarding-pass")]
        [AllowAnonymous]
        public async Task<IActionResult> GetBoardingPass(int ticketId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var ticket = await _ticketRepository.GetTicketWithDetailsAsync(ticketId);

            if (ticket == null)
            {
                return NotFound("Ticket not found.");
            }

            if (!string.IsNullOrWhiteSpace(userId))
            {
                if (ticket.UserId != userId)
                {
                    return Forbid(); // nao é dono do bilhete
                }
            }


            var dto = new BoardingPassDto
            {
                TicketId = ticket.Id,
                PassengerName = ticket.User.FullName,
                FlightNumber = ticket.Flight.FlightNumber,
                FromAirportIATA = ticket.Flight.OriginAirport.IATA,
                FromAirportFullName = ticket.Flight.OriginAirport.FullName,
                ToAirportIATA = ticket.Flight.DestinationAirport.IATA,
                ToAirportFullName = ticket.Flight.DestinationAirport.FullName,
                DepartureUtc = ticket.Flight.DepartureUtc,
                ArrivalUtc = ticket.Flight.ArrivalUtc,
                SeatNumber = $"{ticket.Seat.Row}{ticket.Seat.Letter}",
                SeatClass = ticket.Seat.Class.ToString(),
                TicketBarcodeValue = $"TKT{ticket.Id:D8}FLT{ticket.FlightId:D5}"
            };

            return Ok(dto);
        }



        [HttpGet("{ticketId}/download-pdf")]
        [AllowAnonymous]
        public async Task<IActionResult> DownloadTicketPdf(int ticketId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var ticket = await _ticketRepository.GetTicketWithDetailsAsync(ticketId);

            if (ticket == null || ticket.UserId != userId)
            {
                return Forbid();
            }

            var pdfBytes = await _ticketPdfService.GenerateTicketPdfAsync(ticket);

            return File(pdfBytes, "application/pdf", $"VitoriaAirlines-Ticket-{ticket.Id}.pdf");
        }
    }
}