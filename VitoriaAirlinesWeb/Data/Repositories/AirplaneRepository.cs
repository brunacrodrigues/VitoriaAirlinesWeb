using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using VitoriaAirlinesWeb.Data.Entities;
using VitoriaAirlinesWeb.Data.Enums;
using VitoriaAirlinesWeb.Models.ViewModels.Airplanes;
using VitoriaAirlinesWeb.Models.ViewModels.Dashboard;

namespace VitoriaAirlinesWeb.Data.Repositories
{
    /// <summary>
    /// Provides data access methods for Airplane entities, extending GenericRepository.
    /// </summary>
    public class AirplaneRepository : GenericRepository<Airplane>, IAirplaneRepository
    {
        private readonly DataContext _context;

        /// <summary>
        /// Initializes a new instance of the AirplaneRepository.
        /// </summary>
        /// <param name="context">The data context for database operations.</param>
        public AirplaneRepository(DataContext context) : base(context)
        {
            _context = context;
        }



        /// <summary>
        /// Retrieves an airplane by its ID, including its associated seats.
        /// </summary>
        /// <param name="id">The ID of the airplane.</param>
        /// <returns>
        /// Task: The Airplane entity with seats, or null if not found.
        /// </returns>
        public async Task<Airplane?> GetByIdWithSeatsAsync(int id)
        {
            return await _context.Airplanes
                .Include(a => a.Seats)
                .FirstOrDefaultAsync(a => a.Id == id);
        }


        /// <summary>
        /// Adds a list of seats to the database.
        /// </summary>
        /// <param name="seats">The list of Seat entities to add.</param>
        /// <returns>
        /// Task: A Task representing the asynchronous operation.
        /// </returns>
        public async Task AddSeatsAsync(List<Seat> seats)
        {
            await _context.Seats.AddRangeAsync(seats);
            await _context.SaveChangesAsync();
        }


        /// <summary>
        /// Replaces existing seats for an airplane with a new list of seats in a transaction.
        /// Deletes all current seats associated with the airplane, then adds the new ones.
        /// </summary>
        /// <param name="airplaneId">The ID of the airplane whose seats are to be replaced.</param>
        /// <param name="newSeats">The new list of Seat entities to associate with the airplane.</param>
        /// <returns>
        /// Task: A Task representing the asynchronous operation.
        /// </returns>
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


        /// <summary>
        /// Retrieves a list of active airplanes suitable for a combo box, including seat counts.
        /// </summary>
        /// <returns>
        /// Task: An enumerable collection of AirplaneComboViewModel.
        /// </returns>
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


        /// <summary>
        /// Checks if an airplane can be safely deleted. An airplane cannot be deleted
        /// if it has any future scheduled flights.
        /// </summary>
        /// <param name="id">The ID of the airplane.</param>
        /// <returns>
        /// True if the airplane has no future scheduled flights, false otherwise.
        /// </returns>
        public bool CanBeDeleted(int id)
        {
            return !_context.Flights
                .Any(f => f.AirplaneId == id
                        && f.Status == FlightStatus.Scheduled
                        && f.DepartureUtc > DateTime.UtcNow);
        }


        /// <summary>
        /// Checks if an airplane has ever been associated with any flights.
        /// </summary>
        /// <param name="id">The ID of the airplane.</param>
        /// <returns>
        /// True if the airplane has any associated flights, false otherwise.
        /// </returns>
        public bool HasAnyFlights(int id)
        {
            return _context.Flights.Any(f => f.AirplaneId == id);
        }


        /// <summary>
        /// Checks if an airplane has any flights that are not canceled.
        /// </summary>
        /// <param name="id">The ID of the airplane.</param>
        /// <returns>
        /// True if the airplane has any non-canceled flights, false otherwise.
        /// </returns>
        public bool HasAnyNonCanceledFlights(int id)
        {
            return _context.Flights
                .Any(f => f.AirplaneId == id && f.Status != FlightStatus.Canceled);
        }


        /// <summary>
        /// Checks if an airplane has any future scheduled flights.
        /// </summary>
        /// <param name="id">The ID of the airplane.</param>
        /// <returns>
        /// True if the airplane has future scheduled flights, false otherwise.
        /// </returns>
        public bool HasFutureScheduledFlights(int id)
        {
            return _context.Flights
                .Any(f => f.AirplaneId == id &&
                          f.Status == FlightStatus.Scheduled &&
                          f.DepartureUtc > DateTime.UtcNow);
        }


        /// <summary>
        /// Retrieves all airplanes that are currently in 'Active' status.
        /// </summary>
        /// <returns>
        /// An enumerable collection of active Airplane entities.
        /// </returns>
        public IEnumerable<Airplane> GetActiveAirplanes()
        {
            return _context.Airplanes
                .Where(a => a.Status == AirplaneStatus.Active)
                .ToList();
        }


        /// <summary>
        /// Counts the total number of airplanes that are currently 'Active'.
        /// </summary>
        /// <returns>
        /// Task: The total count of active airplanes.
        /// </returns>
        public async Task<int> CountAirplanesAsync()
        {
            return await _context.Airplanes.Where(a => a.Status == AirplaneStatus.Active).CountAsync();
        }


        /// <summary>
        /// Retrieves occupancy statistics for airplanes, including their models and calculated occupancy rates.
        /// Only considers flights with sold tickets.
        /// </summary>
        /// <returns>
        /// Task: A list of AirplaneOccupancyViewModel.
        /// </returns>
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


        /// <summary>
        /// Calculates the average occupancy rate across all non-canceled flights.
        /// </summary>
        /// <returns>
        /// Task: The average occupancy rate as a percentage, or 0 if no relevant flights exist.
        /// </returns>
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


        /// <summary>
        /// Retrieves the airplane model with the most associated non-canceled flights.
        /// </summary>
        /// <returns>
        /// Task: A MostActiveAirplaneViewModel, or null if no active airplanes or flights are found.
        /// </returns>
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


        /// <summary>
        /// Retrieves the airplane model with the lowest occupancy rate among non-canceled flights.
        /// </summary>
        /// <returns>
        /// Task: A LeastOccupiedAirplaneViewModel, or null if no relevant airplanes or flights are found.
        /// </returns>
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
