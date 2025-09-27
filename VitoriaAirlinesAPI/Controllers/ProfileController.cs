using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VitoriaAirlinesAPI.Dtos;
using VitoriaAirlinesWeb.Data.Repositories;
using VitoriaAirlinesWeb.Helpers;

namespace VitoriaAirlinesAPI.Controllers
{
    /// <summary>
    /// API controller for managing the authenticated customer's profile.
    /// </summary>

    [Authorize(
    AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
    Roles = UserRoles.Customer)]
    [Route("api/[controller]")]
    [ApiController]
    public class ProfileController : ControllerBase
    {
        private readonly IUserHelper _userHelper;
        private readonly ICustomerProfileRepository _customerRepository;
        private readonly IBlobHelper _blobHelper;


        public ProfileController(
            IUserHelper userHelper,
            ICustomerProfileRepository customerRepository,
            IBlobHelper blobHelper)
        {
            _userHelper = userHelper;
            _customerRepository = customerRepository;
            _blobHelper = blobHelper;
        }


        /// <summary>
        /// Retrieves the current authenticated customer's profile information.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<CustomerProfileDto>> GetProfile()
        {
            // Obter o email diretamente do claim
            var email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;


            if (string.IsNullOrWhiteSpace(email))
                return Unauthorized("No email found in token.");


            var user = await _userHelper.GetUserByEmailAsync(email);
            if (user == null)
                return NotFound("User not found.");


            var profile = await _customerRepository.GetByUserIdAsync(user.Id);
            if (profile == null)
                return NotFound("Profile not found.");

            var dto = new CustomerProfileDto
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                ProfileImageUrl = user.ImageFullPath,
                CountryId = profile.CountryId,
                PassportNumber = profile.PassportNumber
            };

            return Ok(dto);
        }



        [HttpPut]
        public async Task<IActionResult> UpdateProfile([FromForm] UpdateCustomerProfileDto model)
        {
            var email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

            if (string.IsNullOrWhiteSpace(email))
                return Unauthorized("No email found in token.");


            var user = await _userHelper.GetUserByEmailAsync(email);
            if (user == null)
                return NotFound("User not found.");

            var profile = await _customerRepository.GetByUserIdAsync(user.Id);
            if (profile == null) return NotFound("Profile not found.");

            // Impede apagar nomes
            if (string.IsNullOrWhiteSpace(model.FirstName) || string.IsNullOrWhiteSpace(model.LastName))
                return BadRequest("First name and last name are required.");

            // Impede apagar passaporte se já estiver definido
            if (string.IsNullOrWhiteSpace(model.PassportNumber) && !string.IsNullOrWhiteSpace(profile.PassportNumber))
                return BadRequest("You cannot remove your passport number once it has been set.");

            // Verifica se novo passaporte já está em uso por outro cliente
            if (!string.IsNullOrWhiteSpace(model.PassportNumber))
            {
                var existing = await _customerRepository.GetByPassportAsync(model.PassportNumber);
                if (existing != null && existing.Id != profile.Id)
                    return BadRequest("Passport number already in use.");
            }

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;

            if (model.RemoveImage)
            {
                user.ProfileImageId = null;
            }
            else if (model.ProfileImage != null && model.ProfileImage.Length > 0)
            {
                var imageId = await _blobHelper.UploadBlobAsync(model.ProfileImage, "images");
                user.ProfileImageId = imageId;
            }

            var updateResult = await _userHelper.UpdateUserAsync(user);
            if (!updateResult.Succeeded)
                return BadRequest("Failed to update user data.");

            profile.CountryId = model.CountryId;
            profile.PassportNumber = model.PassportNumber;

            await _customerRepository.UpdateAsync(profile);

            return Ok(new { message = "Profile updated successfully." });
        }




        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

            if (string.IsNullOrWhiteSpace(email))
                return Unauthorized("No email found in token.");


            var user = await _userHelper.GetUserByEmailAsync(email);
            if (user == null)
                return NotFound("User not found.");


            var result = await _userHelper.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
            if (!result.Succeeded)
                return BadRequest(result.Errors.FirstOrDefault()?.Description ?? "Password change failed.");

            return Ok(new { message = "Password changed successfully." });
        }
    }
}


