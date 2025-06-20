
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

        public async Task UpdateCompletedFlightsAsync()
        {
            var flights = await _context.Flights
                .Where(f => f.Status == FlightStatus.Scheduled && f.ArrivalUtc <= DateTime.UtcNow)
                .ToListAsync();


            foreach (var flight in flights)
            {
                flight.Status = FlightStatus.Completed;
            }

            if (flights.Any())
            {
                await _context.SaveChangesAsync();
            }
        }
    }
}
