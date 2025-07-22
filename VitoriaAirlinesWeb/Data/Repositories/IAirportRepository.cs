using VitoriaAirlinesWeb.Data.Entities;
using VitoriaAirlinesWeb.Models.Airports;

namespace VitoriaAirlinesWeb.Data.Repositories
{
    public interface IAirportRepository : IGenericRepository<Airport>
    {
        IQueryable<Airport> GetAllWithCountries();

        Task<Airport?> GetByIdWithCountryAsync(int id);

        Task<IEnumerable<AirportDropdownViewModel>> GetComboAirportsWithFlagsAsync();

        Task<bool> HasAssociatedFlightsAsync(int id);
    }
}
