using Microsoft.EntityFrameworkCore;
using VitoriaAirlinesWeb.Data.Entities;
using VitoriaAirlinesWeb.Data.Enums;

namespace VitoriaAirlinesWeb.Data.Repositories
{
    public class FlightRepository : GenericRepository<Flight>, IFlightRepository
    {
        private readonly DataContext _context;

        public FlightRepository(DataContext context) : base(context)
        {
            _context = context;
        }


        public IEnumerable<Flight> GetAllWithDetailsAsync()
        {
            return _context.Flights
                .Include(f => f.Airplane)
                .Include(f => f.OriginAirport)
                .ThenInclude(a => a.Country)
                .Include(f => f.DestinationAirport)
                .ThenInclude(a => a.Country)
                .OrderBy(f => f.DepartureUtc)
                .ToList();
        }

        public async Task<Flight?> GetByIdWithDetailsAsync(int id)
        {
            return await _context.Flights
                .Include(f => f.Airplane)
                .Include(f => f.OriginAirport)
                .ThenInclude(a => a.Country)
                .Include(f => f.DestinationAirport)
                .ThenInclude(a => a.Country)
                .FirstOrDefaultAsync(f => f.Id == id);
        }

        public async Task<IEnumerable<Flight>> GetFutureFlightsAsync()
        {
            return await _context.Flights
                .Where(f => f.DepartureUtc > DateTime.UtcNow && f.Status == FlightStatus.Scheduled)
                .Include(f => f.Airplane)
                .Include(f => f.OriginAirport)
                .Include(f => f.DestinationAirport)
                .OrderBy(f => f.DepartureUtc)
                .ToListAsync();
        }


        public async Task<IEnumerable<Flight>> SearchFlightsAsync(DateTime? date, int? originId, int? destinationId)
        {
            var query = _context.Flights
                .Include(f => f.OriginAirport)
                .Include(f => f.DestinationAirport)
                .Include(f => f.Airplane) // TODO incluir os tickets depois
                .AsQueryable();

            if (date is not null)
            {
                query = query.Where(f => f.DepartureUtc.Date == date.Value.Date);
            }

            if (originId is not null && originId > 0)
            {
                query = query.Where(f => f.OriginAirportId == originId.Value);
            }

            if (destinationId is not null && destinationId > 0)
            {
                query = query.Where(f => f.DestinationAirportId == destinationId.Value);
            }

            return await query.OrderBy(f => f.DepartureUtc).ToListAsync();

        }
    }
}
