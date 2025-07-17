using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VitoriaAirlinesWeb.Data.Repositories;
using VitoriaAirlinesWeb.Helpers;
using VitoriaAirlinesWeb.Models.ViewModels.Dashboard;

namespace VitoriaAirlinesWeb.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly IUserHelper _userHelper;
        private readonly IFlightRepository _flightRepository;
        private readonly ITicketRepository _ticketRepository;
        private readonly IAirplaneRepository _airplaneRepository;
        private readonly ICustomerProfileRepository _customerRepository;

        public DashboardController(
            IUserHelper userHelper,
            IFlightRepository flightRepository,
            ITicketRepository ticketRepository,
            IAirplaneRepository airplaneRepository,
            ICustomerProfileRepository customerRepository)
        {
            _userHelper = userHelper;
            _flightRepository = flightRepository;
            _ticketRepository = ticketRepository;
            _airplaneRepository = airplaneRepository;
            _customerRepository = customerRepository;
        }

        public async Task<IActionResult> Index()
        {
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _userHelper.GetUserByEmailAsync(User.Identity.Name);
            if (user == null)
            {
                return new NotFoundViewResult("Error404");
            }

            if (User.IsInRole(UserRoles.Admin))
            {
                ViewData["Role"] = UserRoles.Admin;

                var model = new AdminDashboardViewModel
                {
                    TotalFlights = await _flightRepository.CountFlightsAsync(),
                    TotalTickets = await _ticketRepository.CountTicketsAsync(),
                    TotalEmployees = await _userHelper.CountUsersInRoleAsync(UserRoles.Employee),
                    TotalCustomers = await _userHelper.CountUsersInRoleAsync(UserRoles.Customer),
                    TotalAirplanes = await _airplaneRepository.CountAirplanesAsync(),
                    TotalRevenue = await _ticketRepository.GetTotalRevenueAsync(),
                    ScheduledFlights = await _flightRepository.GetScheduledFlightsAsync(),
                    TicketSalesLast7Days = await _ticketRepository.GetTicketSalesLast7DaysAsync(),
                    AirplaneOccupancyStats = await _airplaneRepository.GetAirplaneOccupancyStatsAsync(),
                    AverageOccupancyRate = await _airplaneRepository.GetAverageOccupancyRateAsync(),
                    MostActiveAirplane = await _airplaneRepository.GetMostActiveAirplaneAsync(),
                    LeastOccupiedAirplane = await _airplaneRepository.GetLeastOccupiedModelAsync(),
                    TopDestinations = await _ticketRepository.GetTopDestinationsAsync(),

                };

                return View("Index", model);
            }

            if (User.IsInRole(UserRoles.Employee))
            {
                ViewData["Role"] = UserRoles.Employee;

                var model = new EmployeeDashboardViewModel
                {
                    TotalScheduledFlights = await _flightRepository.CountScheduledFlightsAsync(),
                    TotalCompletedFlights = await _flightRepository.CountCompletedFlightsAsync(),
                    TotalTicketsSold = await _ticketRepository.CountTicketsLast7DaysAsync(),
                    AverageOccupancy = await _airplaneRepository.GetAverageOccupancyRateAsync(),
                    ScheduledFlights = await _flightRepository.GetScheduledFlightsAsync(),
                    RecentFlights = await _flightRepository.GetRecentFlightsAsync(10),
                    LowOccupancyFlights = await _flightRepository.GetLowOccupancyUpcomingFlightsAsync(),
                };

                return View("Index", model);
            }

            if (User.IsInRole(UserRoles.Customer))
            {
                ViewData["Role"] = UserRoles.Customer;

                var customer = await _customerRepository.GetByUserIdAsync(user.Id);

                var model = new CustomerDashboardViewModel
                {
                    FlightsBooked = await _ticketRepository.CountUserBookedFlightsAsync(user.Id),
                    FlightsCompleted = await _ticketRepository.CountUserCompletedFlightsAsync(user.Id),
                    FlightsCanceled = await _ticketRepository.CountUserCanceledFlightsAsync(user.Id),
                    TotalSpent = await _ticketRepository.GetUserTotalSpentAsync(user.Id),
                    FullName = user.FullName,
                    Email = user.Email,
                    PassportNumber = customer?.PassportNumber,
                    Country = customer?.Country?.Name ?? "Not specified",
                    CountryFlagUrl = customer?.Country?.FlagImageUrl,
                    ProfilePictureUrl = user.ImageFullPath,
                    UpcomingFlights = await _ticketRepository.GetUserUpcomingFlightsAsync(user.Id),
                    PastFlights = await _ticketRepository.GetUserPastFlightsAsync(user.Id),
                    LastCompletedFlight = await _ticketRepository.GetUserLastCompletedFlightAsync(user.Id),


                };


                return View("Index", model);
            }

            return RedirectToAction("Login", "Account");
        }
    }
}
