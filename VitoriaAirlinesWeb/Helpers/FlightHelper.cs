
using Microsoft.EntityFrameworkCore;
using VitoriaAirlinesWeb.Data;

namespace VitoriaAirlinesWeb.Helpers
{
    public class FlightHelper : IFlightHelper
    {
        private readonly DataContext _context;
        private static readonly Random _random = new Random();

        public FlightHelper(DataContext context)
        {
            _context = context;
        }

        public async Task<string> GenerateUniqueFlightNumberAsync()
        {
            string flightNumber;
            do
            {
                flightNumber = $"VA{_random.Next(1000, 10000)}";
            }
            
            while (await _context.Flights.AnyAsync(f => f.FlightNumber == flightNumber));

            return flightNumber;
        }
    }
}
