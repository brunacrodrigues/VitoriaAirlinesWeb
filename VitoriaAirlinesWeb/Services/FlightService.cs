
using Microsoft.EntityFrameworkCore;
using VitoriaAirlinesWeb.Data;
using VitoriaAirlinesWeb.Data.Enums;

namespace VitoriaAirlinesWeb.Services
{
    public class FlightService : IFlightService
    {
        private readonly DataContext _context;

        public FlightService(DataContext context)
        {
            _context = context;
        }

        public async Task UpdateFlightStatusAsync()
        {
            var now = DateTime.UtcNow;

            var flights = await _context.Flights
                .Where(f => f.Status != FlightStatus.Canceled)
                .ToListAsync();

            bool hasChanges = false;

            foreach (var flight in flights)
            {
                if (flight.Status == FlightStatus.Scheduled && flight.DepartureUtc <= now && now < flight.ArrivalUtc)
                {
                    flight.Status = FlightStatus.Departed;
                    hasChanges = true;
                }
                else if (now >= flight.ArrivalUtc && flight.Status != FlightStatus.Completed)
                {
                    flight.Status = FlightStatus.Completed;
                    hasChanges = true;
                }
            }

            if (hasChanges)
            {
                await _context.SaveChangesAsync();
            }
        }

    }
}
