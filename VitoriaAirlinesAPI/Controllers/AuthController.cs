using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using VitoriaAirlinesAPI.Dtos;
using VitoriaAirlinesWeb.Helpers;
using VitoriaAirlinesWeb.Models.ViewModels.Account;

namespace VitoriaAirlinesAPI.Controllers
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
        private readonly IMailHelper _mailHelper;


        /// <summary>
        /// Initializes a new instance of the <see cref="AuthController"/> class.
        /// </summary>
        /// <param name="userHelper">Helper service for user management operations.</param>
        /// <param name="configuration">Application configuration used to retrieve JWT settings.</param>
        public AuthController(
            IUserHelper userHelper,
            IConfiguration configuration,
            IMailHelper mailHelper)
        {
            _userHelper = userHelper;
            _configuration = configuration;
            _mailHelper = mailHelper;
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
                    var result = await _userHelper.ValidatePasswordAsync(user, model.Password);

                    if (result.Succeeded)
                    {
                        var roles = await _userHelper.GetUserRolesAsync(user);

                        var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };

                        foreach (var role in roles)
                        {
                            claims.Add(new Claim(ClaimTypes.Role, role));
                        }

                        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Key"]));
                        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                        var token = new JwtSecurityToken(
                            _configuration["JWT:Issuer"],
                            _configuration["JWT:Audience"],
                            claims,
                            expires: DateTime.UtcNow.AddDays(15),
                            signingCredentials: credentials);

                        var results = new LoginResponse
                        {
                            Token = new JwtSecurityTokenHandler().WriteToken(token),
                            Expiration = token.ValidTo,
                            Role = roles.FirstOrDefault()
                        };

                        return Ok(results);
                    }
                }
            }

            return BadRequest("Failed to login.");
        }




        [HttpPost("forgotpassword")]
        public async Task<IActionResult> ForgotPassword([FromBody] RecoverPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest("Invalid request.");

            var user = await _userHelper.GetUserByEmailAsync(model.Email);
            if (user == null)
                return BadRequest("Invalid request");

            var rawToken = await _userHelper.GeneratePasswordResetTokenAsync(user);
            var tokenBytes = Encoding.UTF8.GetBytes(rawToken);
            var base64Token = WebEncoders.Base64UrlEncode(tokenBytes);

            // codifica só o email manualmente
            var encodedEmail = Uri.EscapeDataString(model.Email);

            // constrói o link na mão
            var deepLink = $"vitoriaairlinesapp://resetpassword?token={base64Token}&email={encodedEmail}";


            var emailBody = $@"
<h1>Password Reset</h1>
<p>Click below to reset your password:</p>
<p>
  <a href=""{deepLink}"" style=""display:inline-block;padding:12px 24px;
    background-color:#4B0012;color:#fff;text-decoration:none;border-radius:4px;white-space:nowrap;"">
    Reset Password
  </a>
</p>";

            var result = await _mailHelper.SendEmailAsync(model.Email, "Password Reset", emailBody);

            if (!result.IsSuccess)
                return StatusCode(500, "Failed to send email.");

            return Ok(new { message = "Reset email sent." });
        }



        [HttpPost("resetpassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest("Invalid request.");

            var user = await _userHelper.GetUserByEmailAsync(model.Username);
            if (user == null)
                return BadRequest("Invalid request");

            var result = await _userHelper.ResetPasswordAsync(user, model.Token, model.Password);
            if (!result.Succeeded)
                return BadRequest("Failed to reset password.");

            return Ok(new { message = "Password has been reset." });
        }

    }
}
