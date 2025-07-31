using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using VitoriaAirlinesWeb.Helpers;
using VitoriaAirlinesWeb.Models.ViewModels.Account;

namespace VitoriaAirlinesWeb.Controllers.API
{
    /// <summary>
    /// API controller responsible for handling authentication-related operations.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserHelper _userHelper;
        private readonly IConfiguration _configuration;


        /// <summary>
        /// Initializes a new instance of the <see cref="AuthController"/> class.
        /// </summary>
        /// <param name="userHelper">Helper service for user management operations.</param>
        /// <param name="configuration">Application configuration used to retrieve JWT settings.</param>
        public AuthController(
            IUserHelper userHelper,
            IConfiguration configuration)
        {
            _userHelper = userHelper;
            _configuration = configuration;
        }


        /// <summary>
        /// Authenticates a user using their email and password, and returns a JWT token if valid.
        /// </summary>
        /// <param name="model">The login credentials (email and password).</param>
        /// <returns>
        /// A 200 OK response containing the JWT token and its expiration time,
        /// or 400 Bad Request if authentication fails.
        /// </returns>
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userHelper.GetUserByEmailAsync(model.Username);
                if (user != null)
                {
                    var result = await _userHelper.ValidatePasswordAsync(
                        user,
                        model.Password);

                    if (result.Succeeded)
                    {
                        var claims = new[]
                        {
                            new Claim(ClaimTypes.NameIdentifier, user.Id),
                            new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                        };

                        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Key"]));
                        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                        var token = new JwtSecurityToken(
                            _configuration["JWT:Issuer"],
                            _configuration["JWT:Audience"],
                            claims,
                            expires: DateTime.UtcNow.AddDays(15),
                            signingCredentials: credentials);

                        var results = new
                        {
                            token = new JwtSecurityTokenHandler().WriteToken(token),
                            expiration = token.ValidTo
                        };

                        return Ok(results);
                    }
                }
            }

            return BadRequest();
        }
    }
}
