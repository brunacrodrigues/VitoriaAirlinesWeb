using Microsoft.EntityFrameworkCore;
using VitoriaAirlinesWeb.Data.Entities;
using VitoriaAirlinesWeb.Models.Airports;

namespace VitoriaAirlinesWeb.Data.Repositories
{
    public class AirportRepository : GenericRepository<Airport>, IAirportRepository
    {
        private readonly DataContext _context;

        public AirportRepository(DataContext context) : base(context)
        {
            _context = context;
        }

        public IQueryable<Airport> GetAllWithCountries()
        {
            return _context.Airports
                .Include(a => a.Country)
                .OrderBy(a => a.Name)
                .AsNoTracking();
        }

        public async Task<Airport?> GetByIdWithCountryAsync(int id)
        {
            return await _context.Airports
                .Include(a => a.Country)
                .FirstOrDefaultAsync(a => a.Id == id);
        }


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


        public async Task<bool> HasAssociatedFlightsAsync(int id)
        {
            return await _context.Flights
                .AnyAsync(f => f.OriginAirportId == id || f.DestinationAirportId == id);
        }


    }
}