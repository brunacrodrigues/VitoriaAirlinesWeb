using Microsoft.AspNetCore.Mvc.Rendering;
using VitoriaAirlinesWeb.Data.Entities;

namespace VitoriaAirlinesWeb.Data.Repositories
{
    /// <summary>
    /// Defines the contract for a repository handling Country entities.
    /// </summary>
    public interface ICountryRepository
    {
        /// <summary>
        /// Retrieves all countries from the database.
        /// </summary>
        /// <returns>IQueryable: A queryable collection of Country entities.</returns>
        IQueryable<Country> GetAll();


        /// <summary>
        /// Retrieves a country by its ID.
        /// </summary>
        /// <param name="id">The ID of the country.</param>
        /// <returns>Task: The Country entity, or null if not found.</returns>
        Task<Country> GetByIdAsync(int id);


        /// <summary>
        /// Retrieves a list of countries suitable for a dropdown selection.
        /// </summary>
        /// <returns>IEnumerable: A collection of SelectListItem, ordered by country name.</returns>
        IEnumerable<SelectListItem> GetComboCountries();
    }
}