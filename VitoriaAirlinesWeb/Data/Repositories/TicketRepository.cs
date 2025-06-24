using Microsoft.EntityFrameworkCore;
using VitoriaAirlinesWeb.Data.Entities;

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
                .Where(t => t.FlightId == flightId)
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
    }
}
