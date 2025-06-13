using Microsoft.AspNetCore.Mvc.Rendering;
using VitoriaAirlinesWeb.Data.Entities;

namespace VitoriaAirlinesWeb.Data.Repositories
{
    public interface ICountryRepository
    {
        IQueryable<Country> GetAll();

        Task<Country> GetByIdAsync(int id);

        IEnumerable<SelectListItem> GetComboCountries();
    }
}
