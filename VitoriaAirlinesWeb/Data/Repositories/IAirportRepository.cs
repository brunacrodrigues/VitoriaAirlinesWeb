using VitoriaAirlinesWeb.Data.Entities;

namespace VitoriaAirlinesWeb.Data.Repositories
{
    public interface IAirportRepository : IGenericRepository<Airport>
    {
        IQueryable<Airport> GetAllWithCountries();

        Task<Airport?> GetByIdWithCountryAsync(int id);
    }
}
