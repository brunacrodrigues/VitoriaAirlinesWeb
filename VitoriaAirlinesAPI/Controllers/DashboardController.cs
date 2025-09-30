using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VitoriaAirlinesAPI.Dtos;
using VitoriaAirlinesWeb.Data.Repositories;
using VitoriaAirlinesWeb.Helpers;
using VitoriaAirlinesWeb.Models.ViewModels.Dashboard;

namespace VitoriaAirlinesAPI.Controllers
{
    [Authorize(
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
        Roles = UserRoles.Customer)]
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly IUserHelper _userHelper;
        private readonly ITicketRepository _ticketRepository;


        public DashboardController(
            IUserHelper userHelper,
            ITicketRepository ticketRepository)
        {
            _userHelper = userHelper;
            _ticketRepository = ticketRepository;
        }


        private FlightSummaryDto? MapFlightInfoToSummary(FlightInfoViewModel? vm)
        {
            if (vm == null) return null;


            return new FlightSummaryDto
            {
                FlightNumber = vm.FlightNumber,
                DepartureTime = vm.DepartureTime,


                OriginIATA = vm.OriginAirportIATA,
                DestinationIATA = vm.DestinationAirportIATA,
                OriginAirportFullName = vm.OriginAirport,
                DestinationAirportFullName = vm.DestinationAirport,


                OriginCountryCode = vm.OriginCountryCode,
                DestinationCountryCode = vm.DestinationCountryCode,

                SeatNumber = vm.SeatNumber ?? "N/A"
            };
        }



        [HttpGet("me")]
        public async Task<IActionResult> GetCustomerDashboard()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userHelper.GetUserByIdAsync(userId);
            if (user == null) return NotFound("User not found.");

            var nextFlightVM = await _ticketRepository.GetUserUpcomingFlightAsync(user.Id);
            var upcomingCount = await _ticketRepository.CountUserUpcomingFlightsAsync(user.Id);
            var completedCount = await _ticketRepository.CountUserCompletedFlightsAsync(user.Id);

            var dto = new CustomerDashboardDto
            {

                FirstName = user.FirstName,
                ProfilePictureUrl = user.ImageFullPath,

                UpcomingFlightsCount = upcomingCount,
                CompletedFlightsCount = completedCount,
                TotalSpent = await _ticketRepository.GetUserTotalSpentAsync(user.Id),

                NextUpcomingFlight = MapFlightInfoToSummary(nextFlightVM)
            };

            return Ok(dto);
        }
    }
}