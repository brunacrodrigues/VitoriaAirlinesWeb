using Microsoft.EntityFrameworkCore;
using VitoriaAirlinesWeb.Data.Entities;
using VitoriaAirlinesWeb.Data.Enums;
using VitoriaAirlinesWeb.Helpers;
using VitoriaAirlinesWeb.Models.ViewModels.Dashboard;

namespace VitoriaAirlinesWeb.Data.Repositories
{
    /// <summary>
    /// Provides data access methods for Flight entities, extending GenericRepository.
    /// </summary>
    public class FlightRepository : GenericRepository<Flight>, IFlightRepository
    {
        private readonly DataContext _context;


        /// <summary>
        /// Initializes a new instance of the FlightRepository.
        /// </summary>
        /// <param name="context">The data context for database operations.</param>
        public FlightRepository(DataContext context) : base(context)
        {
            _context = context;
        }


        /// <summary>
        /// Retrieves all flights including details of associated airplane, origin airport (with country), and destination airport (with country).
        /// Ordered by departure UTC.
        /// </summary>
        /// <returns>
        /// An enumerable collection of Flight entities with loaded navigation properties.
        /// </returns>

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


        /// <summary>
        /// Retrieves a single flight by its ID, including comprehensive details: airplane (with seats),
        /// origin and destination airports (with countries), and tickets (with users).
        /// </summary>
        /// <param name="id">The ID of the flight.</param>
        /// <returns>
        /// Task: The Flight entity with all specified navigation properties loaded, or null if not found.
        /// </returns>
        public async Task<Flight?> GetByIdWithDetailsAsync(int id)
        {
            return await _context.Flights
                .Include(f => f.Airplane).ThenInclude(a => a.Seats)
                .Include(f => f.OriginAirport)
                .ThenInclude(a => a.Country)
                .Include(f => f.DestinationAirport)
                .ThenInclude(a => a.Country)
                .Include(f => f.Tickets).ThenInclude(t => t.User)
                .FirstOrDefaultAsync(f => f.Id == id);
        }


        /// <summary>
        /// Retrieves all scheduled flights that are in the future.
        /// Includes details of associated airplane, origin airport (with country), and destination airport (with country).
        /// Ordered by departure UTC.
        /// </summary>
        /// <returns>
        /// Task: An enumerable collection of future scheduled Flight entities with loaded navigation properties.
        /// </returns>
        public async Task<IEnumerable<Flight>> GetScheduledFlightsAsync()
        {
            return await _context.Flights
                .Where(f => f.DepartureUtc > DateTime.UtcNow && f.Status == FlightStatus.Scheduled)
                .Include(f => f.Airplane)
                .Include(f => f.OriginAirport).ThenInclude(a => a.Country)
                .Include(f => f.DestinationAirport).ThenInclude(a => a.Country)
                .OrderBy(f => f.DepartureUtc)
                .ToListAsync();
        }


        /// <summary>
        /// Searches for scheduled flights based on optional date, origin airport, and destination airport filters.
        /// Only returns future flights.
        /// </summary>
        /// <param name="date">Optional: The specific departure date to search for.</param>
        /// <param name="originId">Optional: The ID of the origin airport.</param>
        /// <param name="destinationId">Optional: The ID of the destination airport.</param>
        /// <returns>
        /// Task: An enumerable collection of matching Flight entities with loaded navigation properties, ordered by departure UTC.
        /// </returns>
        public async Task<IEnumerable<Flight>> SearchFlightsAsync(DateTime? date, int? originId, int? destinationId)
        {
            var query = _context.Flights
                .Where(f => f.DepartureUtc > DateTime.UtcNow && f.Status == FlightStatus.Scheduled)
                .Include(f => f.OriginAirport)
                .Include(f => f.DestinationAirport)
                .Include(f => f.Airplane)
                .AsQueryable();

            if (date is not null)
            {
                var startUtc = TimezoneHelper.ConvertToUtc(date.Value.Date);
                var endUtc = TimezoneHelper.ConvertToUtc(date.Value.Date.AddDays(1).AddTicks(-1));
                query = query.Where(f => f.DepartureUtc >= startUtc && f.DepartureUtc <= endUtc);
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


        /// <summary>
        /// Retrieves a history of flights, including completed, departed, and canceled flights.
        /// Includes details of associated airplane, origin airport (with country), and destination airport (with country).
        /// Ordered by departure UTC in descending order.
        /// </summary>
        /// <returns>
        /// Task: An enumerable collection of past and canceled Flight entities with loaded navigation properties.
        /// </returns>
        public async Task<IEnumerable<Flight>> GetFlightsHistoryAsync()
        {
            return await _context.Flights
                 .Include(f => f.Airplane)
                 .Include(f => f.OriginAirport).ThenInclude(a => a.Country)
                 .Include(f => f.DestinationAirport).ThenInclude(a => a.Country)
                 .Where(f =>
                     f.Status == FlightStatus.Departed ||
                     f.Status == FlightStatus.Completed ||
                     f.Status == FlightStatus.Canceled)
                 .OrderByDescending(f => f.DepartureUtc)
                 .ToListAsync();
        }


        /// <summary>
        /// Retrieves a single flight by its ID, including its associated airplane and seats,
        /// and origin/destination airports with their countries.
        /// </summary>
        /// <param name="id">The ID of the flight.</param>
        /// <returns>
        /// Task: The Flight entity with airplane, seats, and airport details, or null if not found.
        /// </returns>
        public async Task<Flight?> GetByIdWithAirplaneAndSeatsAsync(int id)
        {
            return await _context.Flights
                .Include(f => f.Airplane)
                .ThenInclude(a => a.Seats)
                .Include(f => f.OriginAirport)
                .ThenInclude(a => a.Country)
                .Include(f => f.DestinationAirport)
                .ThenInclude(a => a.Country)
                .FirstOrDefaultAsync(f => f.Id == id);
        }


        /// <summary>
        /// Retrieves all flights scheduled for a specific date (UTC).
        /// </summary>
        /// <param name="date">The date (local time) to search for flights.</param>
        /// <returns>
        /// Task: An enumerable collection of flights scheduled for the given date.
        /// </returns>
        public async Task<IEnumerable<Flight>> GetFlightsForDateAsync(DateTime date)
        {
            var startUtc = TimezoneHelper.ConvertToUtc(date.Date);
            var endUtc = TimezoneHelper.ConvertToUtc(date.Date.AddDays(1).AddTicks(-1));

            return await _context.Flights
                .Where(f => f.DepartureUtc >= startUtc && f.DepartureUtc <= endUtc)
                .Include(f => f.Airplane)
                .Include(f => f.OriginAirport)
                .Include(f => f.DestinationAirport)
                .ToListAsync();
        }



        /// <summary>
        /// Checks if an airplane is available to be assigned to a new flight, considering its existing schedule and maintenance rules.
        /// </summary>
        /// <param name="airplaneId">The ID of the airplane to check.</param>
        /// <param name="newDepartureUtc">The proposed departure time (UTC) for the new flight.</param>
        /// <param name="newDuration">The proposed duration for the new flight.</param>
        /// <param name="newFlightOriginAirportId">The origin airport ID for the new flight.</param>
        /// <param name="flightToEdit">Optional: The ID of a flight currently being edited (to exclude it from conflict checks).</param>
        /// <returns>
        /// Task: True if the airplane is available, false otherwise.
        /// </returns>
        public async Task<bool> IsAirplaneAvailableAsync(int airplaneId,
            DateTime newDepartureUtc,
            TimeSpan newDuration,
            int newFlightOriginAirportId,
            int? flightToEdit = null)
        {

            var flightsWithAirplane = await GetFlightsWithThisAirplane(airplaneId, flightToEdit);

            if (HasScheduleConflict(flightsWithAirplane, newDepartureUtc, newDuration))
                return false;


            if (!IsAirplaneReadyForDeparture(flightsWithAirplane, newDepartureUtc, newFlightOriginAirportId))
                return false;


            return true;

        }


        /// <summary>
        /// Determines if an airplane is ready for departure based on its last completed flight's destination or next scheduled flight's origin.
        /// </summary>
        /// <param name="flightsWithAirplane">Existing flights associated with the airplane.</param>
        /// <param name="newDepartureUtc">The proposed departure time (UTC) for the new flight.</param>
        /// <param name="newFlightOriginAirportId">The origin airport ID for the new flight.</param>
        /// <returns>True if the airplane is positioned correctly for the new departure, false otherwise.</returns>
        private bool IsAirplaneReadyForDeparture(IEnumerable<Flight> flightsWithAirplane, DateTime newDepartureUtc, int newFlightOriginAirportId)
        {
            var lastFlight = flightsWithAirplane
                .Where(f => f.ArrivalUtc <= newDepartureUtc && f.Status != FlightStatus.Canceled)
                .OrderByDescending(f => f.ArrivalUtc)
                .FirstOrDefault();

            if (lastFlight is not null)
                return lastFlight.DestinationAirportId == newFlightOriginAirportId;

            var nextFlight = flightsWithAirplane
                .Where(f => f.DepartureUtc >= newDepartureUtc && f.Status != FlightStatus.Canceled)
                .OrderBy(f => f.DepartureUtc)
                .FirstOrDefault();

            if (nextFlight is not null)
            {
                var hoursUntilNext = (nextFlight.DepartureUtc - newDepartureUtc).TotalHours;
                if (hoursUntilNext >= 6)
                    return true;

                return nextFlight.OriginAirportId == newFlightOriginAirportId;
            }

            return true;
        }



        /// <summary>
        /// Checks if a new flight's schedule conflicts with existing flights for the same airplane, considering minimum ground time.
        /// </summary>
        /// <param name="flightsWithAirplane">Existing flights associated with the airplane.</param>
        /// <param name="newDepartureUtc">The proposed departure time (UTC) for the new flight.</param>
        /// <param name="newDuration">The proposed duration for the new flight.</param>
        /// <returns>True if a schedule conflict is detected, false otherwise.</returns>
        private bool HasScheduleConflict(IEnumerable<Flight> flightsWithAirplane, DateTime newDepartureUtc, TimeSpan newDuration)
        {
            // Business Rule: The minimum time an airplane must be on the ground between flights.
            var minGroundTime = TimeSpan.FromMinutes(90);
            var newArrivalUtc = newDepartureUtc.Add(newDuration);

            foreach (var existingFlight in flightsWithAirplane)
            {
                // The time when the airplane is actually free after completing an existing flight.
                var airplaneAvailableAt = existingFlight.ArrivalUtc.Add(minGroundTime);

                // Conflicts if:
                // The new flight tries to depart BEFORE the airplane is free from the previous flight. AND
                // The new flight's arrival happens AFTER the existing flight has already departed.
                if (newDepartureUtc < airplaneAvailableAt && newArrivalUtc > existingFlight.DepartureUtc)
                    return true;

            }

            return false;
        }


        /// <summary>
        /// Retrieves all non-canceled flights for a specific airplane, optionally excluding a flight being edited.
        /// </summary>
        /// <param name="airplaneId">The ID of the airplane.</param>
        /// <param name="flightToEdit">Optional: The ID of a flight to exclude from the results (if it's being edited).</param>
        /// <returns>Task: A list of Flight entities.</returns>
        private async Task<List<Flight>> GetFlightsWithThisAirplane(int airplaneId, int? flightToEdit)
        {
            return await _context.Flights.
                Where(f => f.AirplaneId == airplaneId &&
                f.Status != FlightStatus.Canceled &&
                // Exclude the flight we might be currently editing.
                (flightToEdit == null || f.Id != flightToEdit))
                .OrderBy(f => f.DepartureUtc)
                .ToListAsync();
        }


        /// <summary>
        /// Retrieves future scheduled flights for a specific airplane that have at least one ticket sold.
        /// </summary>
        /// <param name="airplaneId">The ID of the airplane.</param>
        /// <returns>
        /// Task: An enumerable collection of relevant Flight entities.
        /// </returns>
        public async Task<IEnumerable<Flight>> GetFutureFlightsWithSoldTicketsAsync(int airplaneId)
        {
            return await _context.Flights
                .Where(f =>
                f.AirplaneId == airplaneId &&
                f.DepartureUtc > DateTime.UtcNow &&
                f.Status == FlightStatus.Scheduled &&
                f.Tickets.Any())
                .Select(f => new Flight
                {
                    Id = f.Id,
                    FlightNumber = f.FlightNumber,
                    DepartureUtc = f.DepartureUtc,
                    AirplaneId = f.AirplaneId
                })
                .ToListAsync();
        }


        /// <summary>
        /// Retrieves all future scheduled flights for a specific airplane, including their tickets,
        /// origin and destination airports (with countries).
        /// </summary>
        /// <param name="airplaneId">The ID of the airplane.</param>
        /// <returns>
        /// Task: An enumerable collection of future scheduled Flight entities for the airplane.
        /// </returns>
        public async Task<IEnumerable<Flight>> GetFutureFlightsByAirplaneAsync(int airplaneId)
        {
            return await _context.Flights
                .Include(f => f.Tickets)
                .Include(f => f.OriginAirport).ThenInclude(a => a.Country)
                .Include(f => f.DestinationAirport).ThenInclude(a => a.Country)
                .Where(f =>
                   f.AirplaneId == airplaneId &&
                   f.Status == FlightStatus.Scheduled &&
                   f.DepartureUtc > DateTime.UtcNow)
               .ToListAsync();
        }


        /// <summary>
        /// Counts the total number of flights in the database.
        /// </summary>
        /// <returns>
        /// Task: The total count of flights.
        /// </returns>
        public async Task<int> CountFlightsAsync()
        {
            return await _context.Flights.CountAsync();
        }


        /// <summary>
        /// Counts the number of scheduled flights that are in the future.
        /// </summary>
        /// <returns>
        /// Task: The count of future scheduled flights.
        /// </returns>
        public async Task<int> CountScheduledFlightsAsync()
        {
            return await _context.Flights
                .Where(f => f.DepartureUtc > DateTime.UtcNow && f.Status == FlightStatus.Scheduled)
                .CountAsync();
        }


        /// <summary>
        /// Counts the number of flights that have been completed.
        /// </summary>
        /// <returns>
        /// Task: The count of completed flights.
        /// </returns>
        public async Task<int> CountCompletedFlightsAsync()
        {
            return await _context.Flights
                .Where(f => f.Status == FlightStatus.Completed)
                .CountAsync();
        }



        /// <summary>
        /// Retrieves a specified number of most recent flights (by ID), including relevant details.
        /// </summary>
        /// <param name="count">The maximum number of recent flights to retrieve.</param>
        /// <returns>
        /// Task: A list of recent Flight entities with loaded navigation properties.
        /// </returns>
        public async Task<List<Flight>> GetRecentFlightsAsync(int count)
        {
            return await _context.Flights
                .Include(f => f.OriginAirport).ThenInclude(a => a.Country)
                .Include(f => f.DestinationAirport).ThenInclude(a => a.Country)
                .Include(f => f.Airplane)
                .OrderByDescending(f => f.Id)
                .Take(count)
                .ToListAsync();
        }


        /// <summary>
        /// Retrieves upcoming scheduled flights with an occupancy rate below a given threshold.
        /// </summary>
        /// <param name="threshold">The occupancy rate percentage below which flights are considered low occupancy (default is 50.0).</param>
        /// <param name="maxResults">The maximum number of low occupancy flights to return (default is 5).</param>
        /// <returns>
        /// Task: A list of LowOccupancyFlightViewModel.
        /// </returns>
        public async Task<List<LowOccupancyFlightViewModel>> GetLowOccupancyUpcomingFlightsAsync(double threshold = 50.0, int maxResults = 5)
        {
            var now = DateTime.UtcNow;

            var flights = await _context.Flights
                .Where(f => f.Status == FlightStatus.Scheduled && f.DepartureUtc > now)
                .Include(f => f.OriginAirport).ThenInclude(a => a.Country)
                .Include(f => f.DestinationAirport).ThenInclude(a => a.Country)
                .Include(f => f.Tickets)
                .Include(f => f.Airplane)
                .OrderBy(f => f.DepartureUtc)
                .ToListAsync();

            return flights
                .Select(f => new
                {
                    Flight = f,
                    Capacity = f.Airplane.TotalEconomySeats + f.Airplane.TotalExecutiveSeats,
                    Sold = f.Tickets.Count
                })
                .Where(x => x.Capacity > 0 && (x.Sold * 100.0 / x.Capacity) < threshold)
                .OrderBy(x => x.Flight.DepartureUtc)
                .Take(maxResults)
                .Select(x => new LowOccupancyFlightViewModel
                {
                    Id = x.Flight.Id,
                    FlightNumber = x.Flight.FlightNumber,
                    OriginAirportFullName = x.Flight.OriginAirport.FullName,
                    OriginCountryFlagUrl = x.Flight.OriginAirport.Country.FlagImageUrl,
                    DestinationAirportFullName = x.Flight.DestinationAirport.FullName,
                    DestinationCountryFlagUrl = x.Flight.DestinationAirport.Country.FlagImageUrl,
                    DepartureFormatted = TimezoneHelper.ConvertToLocal(x.Flight.DepartureUtc).ToString("HH:mm dd MMM"),
                    OccupancyRate = Math.Round(x.Sold * 100.0 / x.Capacity, 1)
                })
                .ToList();
        }

    }
}
