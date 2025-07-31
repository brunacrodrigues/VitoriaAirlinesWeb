using VitoriaAirlinesWeb.Data.Entities;
using VitoriaAirlinesWeb.Models.Airports;

namespace VitoriaAirlinesWeb.Data.Repositories
{
    /// <summary>
    /// Defines the contract for a repository handling Airport entities.
    /// Extends IGenericRepository for common CRUD operations and adds specific airport-related queries.
    /// </summary>
    public interface IAirportRepository : IGenericRepository<Airport>
    {
        /// <summary>
        /// Retrieves all airports, including their associated country, ordered by name.
        /// </summary>
        /// <returns>IQueryable: A queryable collection of Airport entities with countries.</returns>
        IQueryable<Airport> GetAllWithCountries();


        /// <summary>
        /// Retrieves an airport by its ID, including its associated country.
        /// </summary>
        /// <param name="id">The ID of the airport.</param>
        /// <returns>Task: The Airport entity with country, or null if not found.</returns>
        Task<Airport?> GetByIdWithCountryAsync(int id);


        /// <summary>
        /// Retrieves a list of airports suitable for a dropdown selection, including city, name, and country flag.
        /// </summary>
        /// <returns>Task: An enumerable collection of AirportDropdownViewModel.</returns>
        Task<IEnumerable<AirportDropdownViewModel>> GetComboAirportsWithFlagsAsync();


        /// <summary>
        /// Checks if an airport has any associated flights (either as origin or destination).
        /// </summary>
        /// <param name="id">The ID of the airport.</param>
        /// <returns>Task: True if the airport has associated flights, false otherwise.</returns>
        Task<bool> HasAssociatedFlightsAsync(int id);
    }
}