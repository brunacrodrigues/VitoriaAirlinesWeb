using VitoriaAirlinesWeb.Data.Entities;
using VitoriaAirlinesWeb.Models.Airport;

namespace VitoriaAirlinesWeb.Data.Repositories
{
    public interface IAirportRepository : IGenericRepository<Airport>
    {
        IQueryable<Airport> GetAllWithCountries();

        Task<Airport?> GetByIdWithCountryAsync(int id);

        Task<IEnumerable<AirportDropdownViewModel>> GetComboAirportsWithFlagsAsync();
    }
}
