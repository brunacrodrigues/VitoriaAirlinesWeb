using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VitoriaAirlinesWeb.Data.Entities;

namespace VitoriaAirlinesWeb.Data.Repositories
{
    /// <summary>
    /// Provides data access methods for Country entities.
    /// </summary>
    public class CountryRepository : ICountryRepository
    {
        private readonly DataContext _context;

        /// <summary>
        /// Initializes a new instance of the CountryRepository.
        /// </summary>
        /// <param name="context">The data context for database operations.</param>
        public CountryRepository(DataContext context)
        {
            _context = context;
        }


        /// <summary>
        /// Retrieves all countries from the database. The query is configured for no tracking.
        /// </summary>
        /// <returns>
        /// An IQueryable of Country entities.
        /// </returns>
        public IQueryable<Country> GetAll()
        {
            return _context.Countries.AsNoTracking();
        }


        /// <summary>
        /// Retrieves a country by its ID. The query is configured for no tracking.
        /// </summary>
        /// <param name="id">The ID of the country.</param>
        /// <returns>
        /// Task: The Country entity, or null if not found.
        /// </returns>
        public async Task<Country> GetByIdAsync(int id)
        {
            return await _context.Countries.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);
        }


        /// <summary>
        /// Retrieves a list of countries suitable for a dropdown selection.
        /// Includes a default "Select a country..." option.
        /// </summary>
        /// <returns>
        /// An enumerable collection of SelectListItem, ordered by country name.
        /// </returns>
        public IEnumerable<SelectListItem> GetComboCountries()
        {
            var list = _context.Countries.Select(c => new SelectListItem
            {
                Text = c.Name,
                Value = c.Id.ToString()

            }).OrderBy(l => l.Text).ToList();

            list.Insert(0, new SelectListItem
            {
                Text = "(Select a country...)",
                Value = "0"

            });

            return list;
        }
    }
}
