using Microsoft.EntityFrameworkCore;
using VitoriaAirlinesWeb.Data.Entities;
using VitoriaAirlinesWeb.Data.Enums;
using VitoriaAirlinesWeb.Models.ViewModels.Dashboard;

namespace VitoriaAirlinesWeb.Data.Repositories
{
    public class TicketRepository : GenericRepository<Ticket>, ITicketRepository
    {
        private readonly DataContext _context;

        public TicketRepository(DataContext context) : base(context)
        {
            _context = context;
        }


        public async Task<IEnumerable<Ticket>> GetTicketsByFlightAsync(int flightId)
        {
            return await _context.Tickets
                .Include(t => t.User)
                .Include(t => t.Seat)
                .Where(t => t.FlightId == flightId && !t.IsCanceled)
                .ToListAsync();
        }

        public async Task<IEnumerable<Ticket>> GetTicketsByUserAsync(string userId)
        {
            return await _context.Tickets
                .Include(t => t.Flight).ThenInclude(f => f.OriginAirport)
                .Include(t => t.Flight).ThenInclude(f => f.DestinationAirport)
                .Include(t => t.Seat)
                .Where(t => t.UserId == userId)
                .ToListAsync();
        }

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

        public async Task<Ticket?> GetTicketWithDetailsAsync(int ticketId)
        {
            return await _context.Tickets
                .Include(t => t.Flight).ThenInclude(f => f.Airplane).ThenInclude(a => a.Seats)
                .Include(t => t.Flight).ThenInclude(f => f.OriginAirport).ThenInclude(a => a.Country)
                .Include(t => t.Flight).ThenInclude(f => f.DestinationAirport).ThenInclude(a => a.Country)
                .Include(t => t.Seat)
                .FirstOrDefaultAsync(t => t.Id == ticketId);
        }

        public async Task<bool> UserHasTicketForFlightAsync(string userId, int flightId)
        {
            return await _context.Tickets.
                AnyAsync(t => t.UserId == userId &&
                t.FlightId == flightId);
        }

        public async Task<int> CountTicketsAsync()
        {
            return await _context.Tickets.CountAsync();
        }


        public async Task<decimal> GetTotalRevenueAsync()
        {
            return await _context.Tickets.SumAsync(t => t.PricePaid);
        }

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


        public async Task<int> CountTicketsLast7DaysAsync()
        {
            var today = DateTime.UtcNow.Date;

            return await _context.Tickets
                .Where(t => t.PurchaseDateUtc >= today.AddDays(-6))
                .CountAsync();
        }


        public async Task<int> CountUserBookedFlightsAsync(string userId)
        {
            return await _context.Tickets
                .Where(t => t.UserId == userId)
                .CountAsync();
        }


        public async Task<int> CountUserCompletedFlightsAsync(string userId)
        {
            return await _context.Tickets
                .Where(t => t.UserId == userId && t.Flight.Status == FlightStatus.Completed)
                .CountAsync();
        }

        public async Task<int> CountUserCanceledFlightsAsync(string userId)
        {
            return await _context.Tickets
                .Where(t => t.UserId == userId && t.Flight.Status == FlightStatus.Canceled)
                .CountAsync();
        }

        public async Task<decimal> GetUserTotalSpentAsync(string userId)
        {
            return await _context.Tickets
                .Where(t => t.UserId == userId)
                .SumAsync(t => (decimal?)t.PricePaid) ?? 0;
        }


        public async Task<List<FlightInfoViewModel>> GetUserUpcomingFlightsAsync(string userId)
        {
            return await _context.Tickets
                .Where(t => t.UserId == userId && t.Flight.DepartureUtc > DateTime.UtcNow)
                .Include(t => t.Flight)
                    .ThenInclude(f => f.OriginAirport)
                        .ThenInclude(a => a.Country)
                .Include(t => t.Flight)
                    .ThenInclude(f => f.DestinationAirport)
                        .ThenInclude(a => a.Country)
                .OrderBy(t => t.Flight.DepartureUtc)
                .Select(t => new FlightInfoViewModel
                {
                    FlightNumber = t.Flight.FlightNumber,
                    OriginAirport = t.Flight.OriginAirport.FullName,
                    OriginCountryFlagUrl = t.Flight.OriginAirport.Country.FlagImageUrl,
                    DestinationAirport = t.Flight.DestinationAirport.FullName,
                    DestinationCountryFlagUrl = t.Flight.DestinationAirport.Country.FlagImageUrl,
                    DepartureTime = t.Flight.DepartureUtc,
                    ArrivalTime = t.Flight.ArrivalUtc
                })
                .ToListAsync();
        }


        public async Task<List<FlightInfoViewModel>> GetUserPastFlightsAsync(string userId)
        {
            return await _context.Tickets
                .Where(t => t.UserId == userId && t.Flight.DepartureUtc <= DateTime.UtcNow)
                .Include(t => t.Flight)
                    .ThenInclude(f => f.OriginAirport)
                        .ThenInclude(a => a.Country)
                .Include(t => t.Flight)
                    .ThenInclude(f => f.DestinationAirport)
                        .ThenInclude(a => a.Country)
                .OrderByDescending(t => t.Flight.DepartureUtc)
                .Select(t => new FlightInfoViewModel
                {
                    FlightNumber = t.Flight.FlightNumber,
                    OriginAirport = t.Flight.OriginAirport.FullName,
                    OriginCountryFlagUrl = t.Flight.OriginAirport.Country.FlagImageUrl,
                    DestinationAirport = t.Flight.DestinationAirport.FullName,
                    DestinationCountryFlagUrl = t.Flight.DestinationAirport.Country.FlagImageUrl,
                    DepartureTime = t.Flight.DepartureUtc,
                    ArrivalTime = t.Flight.ArrivalUtc
                })
                .ToListAsync();
        }

        public async Task<FlightInfoViewModel?> GetUserLastCompletedFlightAsync(string userId)
        {
            var ticket = await _context.Tickets
                .Include(t => t.Flight)
                    .ThenInclude(f => f.OriginAirport)
                        .ThenInclude(a => a.Country)
                .Include(t => t.Flight)
                    .ThenInclude(f => f.DestinationAirport)
                        .ThenInclude(a => a.Country)
                .Where(t => t.UserId == userId && t.Flight.Status == FlightStatus.Completed)
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
                DepartureTime = ticket.Flight.DepartureUtc,
                ArrivalTime = ticket.Flight.DepartureUtc + ticket.Flight.Duration
            };
        }



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
                .Where(t => t.UserId == userId && t.Flight.Status == FlightStatus.Scheduled)
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
                DepartureTime = ticket.Flight.DepartureUtc,
                ArrivalTime = ticket.Flight.DepartureUtc + ticket.Flight.Duration,
                SeatNumber = $"{ticket.Seat.Row}{ticket.Seat.Letter} {ticket.Seat.Class}"
            };
        }


    }
}
