using Microsoft.EntityFrameworkCore;
using VitoriaAirlinesWeb.Data.Entities;
using VitoriaAirlinesWeb.Data.Enums;
using VitoriaAirlinesWeb.Helpers;
using VitoriaAirlinesWeb.Models.ViewModels.Dashboard.VitoriaAirlinesWeb.Models.ViewModels.Dashboard;

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
                .Include(f => f.Airplane).ThenInclude(a => a.Seats)
                .Include(f => f.OriginAirport)
                .ThenInclude(a => a.Country)
                .Include(f => f.DestinationAirport)
                .ThenInclude(a => a.Country)
                .Include(f => f.Tickets).ThenInclude(t => t.User)
                .FirstOrDefaultAsync(f => f.Id == id);
        }


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

        private bool IsAirplaneReadyForDeparture(IEnumerable<Flight> flightsWithAirplane, DateTime newDepartureUtc, int newFlightOriginAirportId)
        {
            // Find the last flight that the airplane completed before the new departure time.
            var lastFlight = flightsWithAirplane.
                Where(f => f.ArrivalUtc <= newDepartureUtc &&
                f.Status != FlightStatus.Canceled)
                .OrderByDescending(f => f.ArrivalUtc)
                .FirstOrDefault();

            // If there was a previous flight
            if (lastFlight is not null)
                return lastFlight.DestinationAirportId == newFlightOriginAirportId; // Origin must match last flight destination airport


            // Check if there is a future flight already scheduled with this airplane
            var nextFlight = flightsWithAirplane
                .Where(f => f.DepartureUtc >= newDepartureUtc && f.Status != FlightStatus.Canceled)
                .OrderBy(f => f.DepartureUtc)
                .FirstOrDefault();

            if (nextFlight is not null)
                return nextFlight.OriginAirportId == newFlightOriginAirportId;


            // If there's no flight history, we assume the airplane is correctly positioned.
            return true;
        }


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


        public async Task<int> CountFlightsAsync()
        {
            return await _context.Flights.CountAsync();
        }


        public async Task<int> CountScheduledFlightsAsync()
        {
            return await _context.Flights
                .Where(f => f.DepartureUtc > DateTime.UtcNow && f.Status == FlightStatus.Scheduled)
                .CountAsync();
        }


        public async Task<int> CountCompletedFlightsAsync()
        {
            return await _context.Flights
                .Where(f => f.Status == FlightStatus.Completed)
                .CountAsync();
        }


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
