using Microsoft.EntityFrameworkCore;
using VitoriaAirlinesWeb.Data.Entities;
using VitoriaAirlinesWeb.Data.Enums;
using VitoriaAirlinesWeb.Helpers;
using VitoriaAirlinesWeb.Models.ViewModels.Dashboard;

namespace VitoriaAirlinesWeb.Data.Repositories
{
    /// <summary>
    /// Provides data access methods for Ticket entities, extending GenericRepository.
    /// </summary>
    public class TicketRepository : GenericRepository<Ticket>, ITicketRepository
    {
        private readonly DataContext _context;


        /// <summary>
        /// Initializes a new instance of the TicketRepository.
        /// </summary>
        /// <param name="context">The data context for database operations.</param>
        public TicketRepository(DataContext context) : base(context)
        {
            _context = context;
        }


        /// <summary>
        /// Retrieves all non-canceled tickets associated with a specific flight, including user and seat details.
        /// </summary>
        /// <param name="flightId">The ID of the flight.</param>
        /// <returns>
        /// Task: A collection of Ticket entities for the specified flight.
        /// </returns>
        public async Task<IEnumerable<Ticket>> GetTicketsByFlightAsync(int flightId)
        {
            return await _context.Tickets
                .Include(t => t.User)
                .Include(t => t.Seat)
                .Where(t => t.FlightId == flightId && !t.IsCanceled)
                .ToListAsync();
        }


        /// <summary>
        /// Retrieves all tickets associated with a specific user, including flight, origin, destination, and seat details.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>
        /// Task: A collection of Ticket entities for the specified user.
        /// </returns>
        public async Task<IEnumerable<Ticket>> GetTicketsByUserAsync(string userId)
        {
            return await _context.Tickets
                .Include(t => t.Flight).ThenInclude(f => f.OriginAirport)
                .Include(t => t.Flight).ThenInclude(f => f.DestinationAirport)
                .Include(t => t.Seat)
                .Where(t => t.UserId == userId)
                .ToListAsync();
        }


        /// <summary>
        /// Retrieves the history of past tickets (flights that have departed/completed/canceled) for a specific user.
        /// Includes details of origin/destination airports and their countries, and seat.
        /// Ordered by departure UTC in descending order.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>
        /// Task: A collection of Ticket entities representing past flights for the user.
        /// </returns>
        public async Task<IEnumerable<Ticket>> GetTicketsHistoryByUserAsync(string userId)
        {
            return await _context.Tickets
                .Include(t => t.Flight).ThenInclude(f => f.OriginAirport).ThenInclude(a => a.Country)
                .Include(t => t.Flight).ThenInclude(f => f.DestinationAirport).ThenInclude(a => a.Country)
                .Include(t => t.Seat)
                .Where(t => t.UserId == userId &&
                t.Flight.DepartureUtc <= DateTime.UtcNow &&
                t.Flight.Status != FlightStatus.Scheduled)
                .OrderByDescending(t => t.Flight.DepartureUtc)
                .ToListAsync();
        }


        public async Task<IEnumerable<Ticket>> GetCompletedFlightsByUserAsync(string userId)
        {
            return await _context.Tickets
                .Include(t => t.Flight).ThenInclude(f => f.OriginAirport).ThenInclude(a => a.Country)
                .Include(t => t.Flight).ThenInclude(f => f.DestinationAirport).ThenInclude(a => a.Country)
                .Include(t => t.Seat)
                .Where(t => t.UserId == userId &&
                t.Flight.DepartureUtc <= DateTime.UtcNow &&
                t.Flight.Status == FlightStatus.Completed)
                .OrderByDescending(t => t.Flight.DepartureUtc)
                .ToListAsync();
        }


        /// <summary>
        /// Retrieves all upcoming tickets (scheduled and not canceled, with future departure) for a specific user.
        /// Includes details of origin/destination airports and their countries, and seat.
        /// Ordered by departure UTC in ascending order.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>
        /// Task: A collection of Ticket entities representing upcoming flights for the user.
        /// </returns>
        public async Task<IEnumerable<Ticket>> GetUpcomingTicketsByUserAsync(string userId)
        {
            return await _context.Tickets
                .Include(t => t.Flight).ThenInclude(f => f.OriginAirport).ThenInclude(a => a.Country)
                .Include(t => t.Flight).ThenInclude(f => f.DestinationAirport).ThenInclude(a => a.Country)
                .Include(t => t.Seat)
                .Where(t => t.UserId == userId &&
                !t.IsCanceled &&
                t.Flight.DepartureUtc > DateTime.UtcNow &&
                t.Flight.Status == FlightStatus.Scheduled)
                .OrderBy(t => t.Flight.DepartureUtc)
                .ToListAsync();
        }


        public async Task<IEnumerable<int>> GetFlightIdsForUserAsync(string userId)
        {
            return await _context.Tickets
                .Where(t => t.UserId == userId && !t.IsCanceled)
                .Select(t => t.FlightId)
                .Distinct()
                .ToListAsync();
        }



        /// <summary>
        /// Retrieves a single ticket by its ID, including comprehensive details of associated entities:
        /// Flight (with Airplane and Seats), Origin and Destination Airports (with Countries), and Seat.
        /// </summary>
        /// <param name="ticketId">The ID of the ticket.</param>
        /// <returns>
        /// Task: The Ticket entity with all specified navigation properties loaded, or null if not found.
        /// </returns>
        public async Task<Ticket?> GetTicketWithDetailsAsync(int ticketId)
        {
            return await _context.Tickets
                .Include(t => t.Flight).ThenInclude(f => f.Airplane).ThenInclude(a => a.Seats)
                .Include(t => t.Flight).ThenInclude(f => f.OriginAirport).ThenInclude(a => a.Country)
                .Include(t => t.Flight).ThenInclude(f => f.DestinationAirport).ThenInclude(a => a.Country)
                .Include(t => t.Seat)
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Id == ticketId);
        }


        /// <summary>
        /// Checks if a specific user has already booked an active (not canceled) ticket for a given flight.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="flightId">The ID of the flight.</param>
        /// <returns>
        /// Task: True if the user has an active ticket for the flight, false otherwise.
        /// </returns>
        public async Task<bool> UserHasTicketForFlightAsync(string userId, int flightId)
        {
            return await _context.Tickets
                .AnyAsync(t => t.UserId == userId &&
                               t.FlightId == flightId &&
                               !t.IsCanceled);
        }


        /// <summary>
        /// Counts the total number of tickets in the database.
        /// </summary>
        /// <returns>
        /// Task: The total count of tickets.
        /// </returns>
        public async Task<int> CountTicketsAsync()
        {
            return await _context.Tickets.CountAsync();
        }



        /// <summary>
        /// Calculates the total revenue generated from all tickets (sum of PricePaid).
        /// </summary>
        /// <returns>
        /// Task: The total revenue as a decimal.
        /// </returns>
        public async Task<decimal> GetTotalRevenueAsync()
        {
            return await _context.Tickets.SumAsync(t => t.PricePaid);
        }


        /// <summary>
        /// Retrieves ticket sales data for the last 7 days, grouped by day.
        /// </summary>
        /// <returns>
        /// Task: A list of TicketSalesByDayViewModel.
        /// </returns>
        public async Task<List<TicketSalesByDayViewModel>> GetTicketSalesLast7DaysAsync()
        {
            var today = DateTime.UtcNow.Date;

            return await _context.Tickets
                .Where(t => t.PurchaseDateUtc >= today.AddDays(-6))
                .GroupBy(t => t.PurchaseDateUtc.Date)
                .OrderBy(g => g.Key)
                .Select(g => new TicketSalesByDayViewModel
                {
                    Date = g.Key.ToString("dd MMM"),
                    TicketCount = g.Count()
                })
                .ToListAsync();
        }



        /// <summary>
        /// Retrieves a list of top destinations based on ticket sales, limited to the top 3.
        /// Includes city, airport name, country name, and flag URL.
        /// </summary>
        /// <returns>
        /// Task: A list of TopDestinationViewModel.
        /// </returns>
        public async Task<List<TopDestinationViewModel>> GetTopDestinationsAsync()
        {
            var tickets = await _context.Tickets
                .Include(t => t.Flight)
                    .ThenInclude(f => f.DestinationAirport)
                        .ThenInclude(a => a.Country)
                .ToListAsync();

            return tickets
                .Where(t => t.Flight?.DestinationAirport?.Country != null)
                .GroupBy(t => new
                {
                    City = t.Flight.DestinationAirport.City,
                    AirportName = $"{t.Flight.DestinationAirport.IATA} - {t.Flight.DestinationAirport.Name}",
                    CountryName = t.Flight.DestinationAirport.Country.Name,
                    FlagUrl = t.Flight.DestinationAirport.Country.FlagImageUrl
                })
                .Select(g => new TopDestinationViewModel
                {
                    City = g.Key.City,
                    AirportName = g.Key.AirportName,
                    CountryName = g.Key.CountryName,
                    CountryFlagUrl = g.Key.FlagUrl,
                    TicketCount = g.Count()
                })
                .OrderByDescending(x => x.TicketCount)
                .Take(3)
                .ToList();
        }



        /// <summary>
        /// Counts the total number of tickets sold in the last 7 days.
        /// </summary>
        /// <returns>
        /// Task: The count of tickets sold in the last 7 days.
        /// </returns>
        public async Task<int> CountTicketsLast7DaysAsync()
        {
            var today = DateTime.UtcNow.Date;

            return await _context.Tickets
                .Where(t => t.PurchaseDateUtc >= today.AddDays(-6))
                .CountAsync();
        }



        /// <summary>
        /// Counts the total number of flights (tickets) a specific user has booked, regardless of status.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>
        /// Task: The total count of booked flights for the user.
        /// </returns>
        public async Task<int> CountUserBookedFlightsAsync(string userId)
        {
            return await _context.Tickets
                .Where(t => t.UserId == userId)
                .CountAsync();
        }



        /// <summary>
        /// Counts the number of flights a specific user has completed.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>
        /// Task: The count of completed flights for the user.
        /// </returns>
        public async Task<int> CountUserCompletedFlightsAsync(string userId)
        {
            return await _context.Tickets
                .Where(t => t.UserId == userId && t.Flight.Status == FlightStatus.Completed)
                .CountAsync();
        }


        /// <summary>
        /// Counts the number of flights a specific user has canceled (where flight status is Canceled).
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>
        /// Task: The count of canceled flights for the user.
        /// </returns>
        public async Task<int> CountUserCanceledFlightsAsync(string userId)
        {
            return await _context.Tickets
                .Where(t => t.UserId == userId && t.Flight.Status == FlightStatus.Canceled)
                .CountAsync();
        }


        /// <summary>
        /// Calculates the total amount of money a specific user has spent on tickets.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>
        /// Task: The total amount spent by the user, or 0 if no tickets found.
        /// </returns>
        public async Task<decimal> GetUserTotalSpentAsync(string userId)
        {
            return await _context.Tickets
                .Where(t => t.UserId == userId)
                .SumAsync(t => (decimal?)t.PricePaid) ?? 0;
        }


        /// <summary>
        /// Retrieves a list of upcoming flights for a specific user, formatted as FlightInfoViewModel.
        /// Includes origin/destination airport details and countries.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>
        /// Task: A list of FlightInfoViewModel for upcoming flights.
        /// </returns>
        public async Task<List<FlightInfoViewModel>> GetUserUpcomingFlightsAsync(string userId)
        {
            var tickets = await _context.Tickets
                .Where(t => t.UserId == userId &&
                            !t.IsCanceled &&
                            t.Flight.DepartureUtc > DateTime.UtcNow &&
                            t.Flight.Status == FlightStatus.Scheduled)
                .Include(t => t.Flight)
                    .ThenInclude(f => f.OriginAirport)
                        .ThenInclude(a => a.Country)
                .Include(t => t.Flight)
                    .ThenInclude(f => f.DestinationAirport)
                        .ThenInclude(a => a.Country)
                        .OrderBy(t => t.Flight.DepartureUtc)
                .ToListAsync();

            return tickets.Select(t => new FlightInfoViewModel
            {
                FlightNumber = t.Flight.FlightNumber,
                OriginAirport = t.Flight.OriginAirport.FullName,
                OriginCountryFlagUrl = t.Flight.OriginAirport.Country.FlagImageUrl,
                DestinationAirport = t.Flight.DestinationAirport.FullName,
                DestinationCountryFlagUrl = t.Flight.DestinationAirport.Country.FlagImageUrl,
                DepartureTime = TimezoneHelper.ConvertToLocal(t.Flight.DepartureUtc),
                ArrivalTime = TimezoneHelper.ConvertToLocal(t.Flight.DepartureUtc + t.Flight.Duration)
            }).ToList();
        }


        /// <summary>
        /// Retrieves a list of past flights for a specific user, formatted as FlightInfoViewModel.
        /// Includes origin/destination airport details and countries.
        /// Ordered by departure UTC in descending order.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>
        /// Task: A list of FlightInfoViewModel for past flights.
        /// </returns>
        public async Task<List<FlightInfoViewModel>> GetUserPastFlightsAsync(string userId)
        {
            var tickets = await _context.Tickets
                .Where(t => t.UserId == userId &&
                            !t.IsCanceled &&
                            t.Flight.Status != FlightStatus.Canceled &&
                            t.Flight.DepartureUtc <= DateTime.UtcNow)
                .Include(t => t.Flight)
                    .ThenInclude(f => f.OriginAirport)
                        .ThenInclude(a => a.Country)
                .Include(t => t.Flight)
                    .ThenInclude(f => f.DestinationAirport)
                        .ThenInclude(a => a.Country)
                .OrderByDescending(t => t.Flight.DepartureUtc)
                .ToListAsync();

            return tickets.Select(t => new FlightInfoViewModel
            {
                FlightNumber = t.Flight.FlightNumber,
                OriginAirport = t.Flight.OriginAirport.FullName,
                OriginCountryFlagUrl = t.Flight.OriginAirport.Country.FlagImageUrl,
                DestinationAirport = t.Flight.DestinationAirport.FullName,
                DestinationCountryFlagUrl = t.Flight.DestinationAirport.Country.FlagImageUrl,
                DepartureTime = TimezoneHelper.ConvertToLocal(t.Flight.DepartureUtc),
                ArrivalTime = TimezoneHelper.ConvertToLocal(t.Flight.DepartureUtc + t.Flight.Duration)
            }).ToList();
        }


        /// <summary>
        /// Retrieves the most recent completed flight for a specific user, formatted as FlightInfoViewModel.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>
        /// Task: A FlightInfoViewModel for the last completed flight, or null if none found.
        /// </returns>
        public async Task<FlightInfoViewModel?> GetUserLastCompletedFlightAsync(string userId)
        {
            var ticket = await _context.Tickets
                .Include(t => t.Flight)
                    .ThenInclude(f => f.OriginAirport)
                        .ThenInclude(a => a.Country)
                .Include(t => t.Flight)
                    .ThenInclude(f => f.DestinationAirport)
                        .ThenInclude(a => a.Country)
                .Where(t => t.UserId == userId &&
                !t.IsCanceled &&
            t.Flight.Status == FlightStatus.Completed)
                .OrderByDescending(t => t.Flight.DepartureUtc)
                .FirstOrDefaultAsync();

            if (ticket == null) return null;

            return new FlightInfoViewModel
            {
                FlightNumber = ticket.Flight.FlightNumber,
                OriginAirport = ticket.Flight.OriginAirport.FullName,
                OriginCountryFlagUrl = ticket.Flight.OriginAirport.Country.FlagImageUrl,
                DestinationAirport = ticket.Flight.DestinationAirport.FullName,
                DestinationCountryFlagUrl = ticket.Flight.DestinationAirport.Country.FlagImageUrl,
                DepartureTime = TimezoneHelper.ConvertToLocal(ticket.Flight.DepartureUtc),
                ArrivalTime = TimezoneHelper.ConvertToLocal(ticket.Flight.DepartureUtc + ticket.Flight.Duration)
            };
        }


        /// <summary>
        /// Retrieves the next upcoming scheduled flight for a specific user, formatted as FlightInfoViewModel.
        /// Includes seat information.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>
        /// Task: A FlightInfoViewModel for the next upcoming flight, or null if none found.
        /// </returns>
        public async Task<FlightInfoViewModel?> GetUserUpcomingFlightAsync(string userId)
        {
            var ticket = await _context.Tickets
                .Include(t => t.Seat)
                .Include(t => t.Flight)
                    .ThenInclude(f => f.OriginAirport)
                        .ThenInclude(a => a.Country)
                .Include(t => t.Flight)
                    .ThenInclude(f => f.DestinationAirport)
                        .ThenInclude(a => a.Country)
                .Where(t => t.UserId == userId &&
                !t.IsCanceled &&
                t.Flight.Status == FlightStatus.Scheduled &&
                t.Flight.DepartureUtc > DateTime.UtcNow)
                .OrderBy(t => t.Flight.DepartureUtc)
                .FirstOrDefaultAsync();

            if (ticket == null) return null;

            return new FlightInfoViewModel
            {
                FlightId = ticket.Flight.Id,
                TicketId = ticket.Id,
                FlightNumber = ticket.Flight.FlightNumber,
                OriginAirport = ticket.Flight.OriginAirport.FullName,
                OriginCountryFlagUrl = ticket.Flight.OriginAirport.Country.FlagImageUrl,
                DestinationAirport = ticket.Flight.DestinationAirport.FullName,
                DestinationCountryFlagUrl = ticket.Flight.DestinationAirport.Country.FlagImageUrl,
                DepartureTime = TimezoneHelper.ConvertToLocal(ticket.Flight.DepartureUtc),
                ArrivalTime = TimezoneHelper.ConvertToLocal(ticket.Flight.DepartureUtc + ticket.Flight.Duration),
                SeatNumber = $"{ticket.Seat.Row}{ticket.Seat.Letter} {ticket.Seat.Class}"
            };
        }


        /// <summary>
        /// Checks if a specific user has any upcoming flights (scheduled and not canceled, with future departure).
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>
        /// Task: True if the user has upcoming flights, false otherwise.
        /// </returns>
        public async Task<bool> HasUpcomingFlightsAsync(string userId)
        {
            return await _context.Tickets
                .Include(t => t.Flight)
                .AnyAsync(t =>
                    t.UserId == userId &&
                    !t.IsCanceled &&
                    t.Flight.DepartureUtc > DateTime.UtcNow &&
                    t.Flight.Status == FlightStatus.Scheduled);
        }


        /// <summary>
        /// Retrieves an active (not canceled) ticket by its seat ID and flight ID.
        /// </summary>
        /// <param name="seatId">The ID of the seat.</param>
        /// <param name="flightId">The ID of the flight.</param>
        /// <returns>
        /// Task: The Ticket entity, or null if not found or canceled.
        /// </returns>
        public async Task<Ticket?> GetBySeatAndFlightAsync(int seatId, int flightId)
        {
            return await _context.Tickets
                .FirstOrDefaultAsync(t => t.SeatId == seatId && t.FlightId == flightId && !t.IsCanceled);
        }




    }
}
