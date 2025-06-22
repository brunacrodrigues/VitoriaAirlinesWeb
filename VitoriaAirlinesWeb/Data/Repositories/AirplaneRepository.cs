using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using VitoriaAirlinesWeb.Data.Entities;
using VitoriaAirlinesWeb.Data.Enums;
using VitoriaAirlinesWeb.Models.Airplanes;

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

        // TODO: Validar voos futuros com bilhetes vendidos para este avião via FlightRepository

    }
}
