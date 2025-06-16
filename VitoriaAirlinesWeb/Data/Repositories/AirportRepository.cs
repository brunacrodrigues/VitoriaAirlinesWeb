using Microsoft.EntityFrameworkCore;
using VitoriaAirlinesWeb.Data.Entities;

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
    }
}
