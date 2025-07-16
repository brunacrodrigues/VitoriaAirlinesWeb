using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using VitoriaAirlinesWeb.Data.Entities;
using VitoriaAirlinesWeb.Data.Enums;
using VitoriaAirlinesWeb.Models.ViewModels.Airplanes;
using VitoriaAirlinesWeb.Models.ViewModels.Dashboard;

namespace VitoriaAirlinesWeb.Data.Repositories
{
    public class AirplaneRepository : GenericRepository<Airplane>, IAirplaneRepository
    {
        private readonly DataContext _context;

        public AirplaneRepository(DataContext context) : base(context)
        {
            _context = context;
        }


        public async Task<Airplane?> GetByIdWithSeatsAsync(int id)
        {
            return await _context.Airplanes
                .Include(a => a.Seats)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task AddSeatsAsync(List<Seat> seats)
        {
            await _context.Seats.AddRangeAsync(seats);
            await _context.SaveChangesAsync();
        }

        public async Task ReplaceSeatsAsync(int airplaneId, List<Seat> newSeats)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // delete existing seats
                var existingSeats = await _context.Seats
                    .Where(s => s.AirplaneId == airplaneId)
                    .ToListAsync();

                _context.Seats.RemoveRange(existingSeats);
                await _context.SaveChangesAsync();

                // add new seats
                await _context.Seats.AddRangeAsync(newSeats);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<IEnumerable<AirplaneComboViewModel>> GetComboAirplanesAsync()
        {
            var list = await _context.Airplanes
                   .OrderBy(a => a.Model)
                   .Where(a => a.Status == AirplaneStatus.Active)
                   .Select(a => new AirplaneComboViewModel
                   {
                       Id = a.Id,
                       Model = a.Model,
                       EconomySeats = a.TotalEconomySeats,
                       ExecutiveSeats = a.TotalExecutiveSeats
                   })
                    .ToListAsync();

            return list;
        }

        public bool CanBeDeleted(int id)
        {
            return !_context.Flights
                .Any(f => f.AirplaneId == id
                        && f.Status == FlightStatus.Scheduled
                        && f.DepartureUtc > DateTime.UtcNow);
        }

        public bool HasAnyFlights(int id)
        {
            return _context.Flights.Any(f => f.AirplaneId == id);
        }

        public bool HasAnyNonCanceledFlights(int id)
        {
            return _context.Flights
                .Any(f => f.AirplaneId == id && f.Status != FlightStatus.Canceled);
        }

        public bool HasFutureScheduledFlights(int id)
        {
            return _context.Flights
                .Any(f => f.AirplaneId == id &&
                          f.Status == FlightStatus.Scheduled &&
                          f.DepartureUtc > DateTime.UtcNow);
        }

        public IEnumerable<Airplane> GetActiveAirplanes()
        {
            return _context.Airplanes
                .Where(a => a.Status == AirplaneStatus.Active)
                .ToList();
        }

        public async Task<int> CountAirplanesAsync()
        {
            return await _context.Airplanes.Where(a => a.Status == AirplaneStatus.Active).CountAsync();
        }


        public async Task<List<AirplaneOccupancyViewModel>> GetAirplaneOccupancyStatsAsync()
        {
            return await _context.Flights
                .Where(f => f.Tickets.Any()) // flights with sold tickets
                .GroupBy(f => new
                {
                    f.Airplane.Id,
                    f.Airplane.Model,
                    f.Airplane.TotalExecutiveSeats,
                    f.Airplane.TotalEconomySeats
                })
                .Select(g => new AirplaneOccupancyViewModel
                {
                    Model = g.Key.Model,
                    OccupancyRate = g.SelectMany(f => f.Tickets).Count() * 100.0 /
                                    (g.Count() * (g.Key.TotalExecutiveSeats + g.Key.TotalEconomySeats))
                })
                .OrderByDescending(x => x.OccupancyRate)
                .ToListAsync();
        }


        public async Task<double> GetAverageOccupancyRateAsync()
        {
            var query = await _context.Flights
                .Where(f => f.Status != FlightStatus.Canceled)
                .Select(f => new
                {
                    Capacity = f.Airplane.TotalEconomySeats + f.Airplane.TotalExecutiveSeats,
                    TicketsSold = f.Tickets.Count()
                })
                .ToListAsync();

            if (!query.Any()) return 0;

            double totalSeats = query.Sum(x => x.Capacity);
            double totalSold = query.Sum(x => x.TicketsSold);

            return (totalSold / totalSeats) * 100;
        }



        public async Task<MostActiveAirplaneViewModel?> GetMostActiveAirplaneAsync()
        {
            return await _context.Flights
                .Where(f => f.Status != FlightStatus.Canceled && f.Airplane != null)
                .GroupBy(f => f.Airplane.Model)
                .Select(g => new MostActiveAirplaneViewModel
                {
                    Model = g.Key,
                    FlightCount = g.Count()
                })
                .OrderByDescending(x => x.FlightCount)
                .FirstOrDefaultAsync();
        }

        public async Task<LeastOccupiedAirplaneViewModel?> GetLeastOccupiedModelAsync()
        {
            return await _context.Flights
                .Where(f => f.Status != FlightStatus.Canceled && f.Airplane != null)
                .GroupBy(f => f.Airplane.Model)
                .Select(g => new
                {
                    Model = g.Key,

                    TotalCapacityAcrossAllFlights = g.Sum(f => f.Airplane.TotalEconomySeats + f.Airplane.TotalExecutiveSeats),
                    TotalTicketsSold = g.SelectMany(f => f.Tickets).Count()
                })
                .Where(x => x.TotalCapacityAcrossAllFlights > 0)
                .Select(x => new LeastOccupiedAirplaneViewModel
                {
                    Model = x.Model,
                    OccupancyRate = (double)x.TotalTicketsSold * 100.0 / x.TotalCapacityAcrossAllFlights
                })
                .OrderBy(x => x.OccupancyRate)
                .FirstOrDefaultAsync();
        }

    }
}
