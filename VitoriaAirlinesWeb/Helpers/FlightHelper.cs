
using Microsoft.EntityFrameworkCore;
using VitoriaAirlinesWeb.Data;

namespace VitoriaAirlinesWeb.Helpers
{
    /// <summary>
    /// Provides helper methods for flight-related operations, such as generating unique flight numbers.
    /// </summary>
    public class FlightHelper : IFlightHelper
    {
        private readonly DataContext _context;
        private static readonly Random _random = new Random();


        /// <summary>
        /// Initializes a new instance of the FlightHelper class.
        /// </summary>
        /// <param name="context">The data context for database operations.</param>
        public FlightHelper(DataContext context)
        {
            _context = context;
        }


        /// <summary>
        /// Asynchronously generates a unique flight number in the format "VAXXXX" (e.g., "VA1234").
        /// It repeatedly generates numbers until one that does not exist in the database is found.
        /// </summary>
        /// <returns>
        /// Task: A unique flight number string.
        /// </returns>
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
