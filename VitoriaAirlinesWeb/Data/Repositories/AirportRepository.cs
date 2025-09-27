using Microsoft.EntityFrameworkCore;
using VitoriaAirlinesWeb.Data.Entities;
using VitoriaAirlinesWeb.Models.Airports;

namespace VitoriaAirlinesWeb.Data.Repositories
{
    /// <summary>
    /// Provides data access methods for Airport entities, extending GenericRepository.
    /// </summary>
    public class AirportRepository : GenericRepository<Airport>, IAirportRepository
    {
        private readonly DataContext _context;


        /// <summary>
        /// Initializes a new instance of the AirportRepository.
        /// </summary>
        /// <param name="context">The data context for database operations.</param>
        public AirportRepository(DataContext context) : base(context)
        {
            _context = context;
        }


        /// <summary>
        /// Retrieves all airports, including their associated country, ordered by name.
        /// The query is configured for no tracking.
        /// </summary>
        /// <returns>
        /// An IQueryable of Airport entities with their Country navigation property loaded.
        /// </returns>
        public IQueryable<Airport> GetAllWithCountries()
        {
            return _context.Airports
                .Include(a => a.Country)
                .OrderBy(a => a.Name)
                .AsNoTracking();
        }


        /// <summary>
        /// Retrieves an airport by its ID, including its associated country.
        /// </summary>
        /// <param name="id">The ID of the airport.</param>
        /// <returns>
        /// Task: The Airport entity with its Country navigation property loaded, or null if not found.
        /// </returns>
        public async Task<Airport?> GetByIdWithCountryAsync(int id)
        {
            return await _context.Airports
                .Include(a => a.Country)
                .FirstOrDefaultAsync(a => a.Id == id);
        }


        /// <summary>
        /// Retrieves a list of airports suitable for a dropdown selection, including city, name, and country flag.
        /// </summary>
        /// <returns>
        /// Task: An enumerable collection of AirportDropdownViewModel.
        /// </returns>
        public async Task<IEnumerable<AirportDropdownViewModel>> GetComboAirportsWithFlagsAsync()
        {
            var list = await _context.Airports
            .Include(a => a.Country)
            .OrderBy(a => a.City)
            .ThenBy(a => a.Name)
            .Select(a => new AirportDropdownViewModel
            {
                Value = a.Id.ToString(),
                Text = a.FullName,
                Icon = a.Country.CountryCode.ToLower()
            })
            .ToListAsync();

            return list;
        }


        /// <summary>
        /// Checks if an airport has any associated flights (either as origin or destination).
        /// </summary>
        /// <param name="id">The ID of the airport.</param>
        /// <returns>
        /// Task: True if the airport has associated flights, false otherwise.
        /// </returns>
        public async Task<bool> HasAssociatedFlightsAsync(int id)
        {
            return await _context.Flights
                .AnyAsync(f => f.OriginAirportId == id || f.DestinationAirportId == id);
        }


        public async Task<List<Airport>> GetAllWithCountryAsync()
        {
            return await _context.Airports
                .Include(a => a.Country)
                .Include(a => a.City)
                .OrderBy(a => a.IATA)
                .ToListAsync();
        }

        public async Task<List<Airport>> SearchAsync(string? term, string? countryCode, int limit = 50)
        {
            var query = _context.Airports
                .AsNoTracking()
                .Include(a => a.Country)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(term))
            {
                var t = term.Trim();
                query = query.Where(a =>
                    EF.Functions.Like(a.IATA, $"%{t}%") ||
                    EF.Functions.Like(a.Name, $"%{t}%") ||
                    EF.Functions.Like(a.City, $"%{t}%") ||
                    EF.Functions.Like(a.Country.Name, $"%{t}%") ||
                    EF.Functions.Like(a.Country.CountryCode, $"%{t}%"));
            }

            if (!string.IsNullOrWhiteSpace(countryCode))
            {
                var cc = countryCode.Trim().ToUpperInvariant();
                query = query.Where(a => a.Country.CountryCode == cc);
            }

            limit = Math.Clamp(limit, 1, 200);

            return await query
                .OrderBy(a => a.IATA)
                .ThenBy(a => a.Name)
                .Take(limit)
                .ToListAsync();
        }
    }
}