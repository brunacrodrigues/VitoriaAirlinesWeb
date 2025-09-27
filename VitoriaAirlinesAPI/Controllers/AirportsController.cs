using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VitoriaAirlinesAPI.Dtos;
using VitoriaAirlinesWeb.Data.Repositories;

namespace VitoriaAirlinesAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class AirportsController : ControllerBase
    {
        private readonly IAirportRepository _airportRepository;

        public AirportsController(IAirportRepository airportRepository)
        {
            _airportRepository = airportRepository;
        }

        /// <summary>
        /// Returns airports for combo pickers (anonymous). No ViewModel used here.
        /// </summary>
        [HttpGet("combo")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(IEnumerable<AirportDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCombo()
        {
            var airports = await _airportRepository
                .GetAllWithCountries()
                .OrderBy(a => a.IATA).ThenBy(a => a.Name)
                .ToListAsync();

            var result = airports.Select(a => new AirportDto
            {
                Id = a.Id,
                IATA = a.IATA,
                Name = a.Name,
                City = a.City,
                CountryCode = a.Country.CountryCode
            });

            return Ok(result);
        }
    }
}
