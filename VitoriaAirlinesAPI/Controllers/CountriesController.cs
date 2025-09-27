using Microsoft.AspNetCore.Mvc;
using VitoriaAirlinesWeb.Data.Entities;
using VitoriaAirlinesWeb.Data.Repositories;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace VitoriaAirlinesAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CountriesController : ControllerBase
    {
        private readonly ICountryRepository _countryRepository;

        public CountriesController(ICountryRepository countryRepository)
        {
            _countryRepository = countryRepository;
        }



        /// <summary>
        /// Returns a list of all countries available in the system.
        /// </summary>
        // GET: api/<CountriesController>
        [HttpGet]
        public ActionResult<IEnumerable<Country>> GetCountries()
        {
            var countries = _countryRepository.GetAll();

            return Ok(countries.Select(c => new
            {
                c.Id,
                c.Name,
                c.CountryCode,
                c.FlagImageUrl
            }));
        }
    }
}
