using Microsoft.EntityFrameworkCore;
using VitoriaAirlinesWeb.Data.Entities;
using VitoriaAirlinesWeb.Data.Enums;

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

    }
}
