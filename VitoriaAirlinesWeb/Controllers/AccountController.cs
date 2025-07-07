using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using VitoriaAirlinesWeb.Data.Entities;
using VitoriaAirlinesWeb.Data.Repositories;
using VitoriaAirlinesWeb.Helpers;
using VitoriaAirlinesWeb.Models.ViewModels.Account;
using VitoriaAirlinesWeb.Responses;

namespace VitoriaAirlinesWeb.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserHelper _userHelper;
        private readonly IMailHelper _mailHelper;
        private readonly IBlobHelper _blobHelper;
        private readonly ICustomerProfileRepository _customerRepository;

        public AccountController(
            IUserHelper userHelper,
            IMailHelper mailHelper,
            IBlobHelper blobHelper,
            ICustomerProfileRepository customerRepository
            )
        {
            _userHelper = userHelper;
            _mailHelper = mailHelper;
            _blobHelper = blobHelper;
            _customerRepository = customerRepository;
        }

        public IActionResult Login()
        {

            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _userHelper.LoginAsync(model);
                if (result.Succeeded)
                {
                    var user = await _userHelper.GetUserByEmailAsync(model.Username);
                    if (user == null)
                    {
                        await _userHelper.LogoutAsync();
                        ModelState.AddModelError(string.Empty, "User not found.");
                        return View(model);
                    }

                    var roles = await _userHelper.GetUserRolesAsync(user);

                    var returnUrl = Request.Query["ReturnUrl"].FirstOrDefault();
                    if (!string.IsNullOrWhiteSpace(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }


                    return RedirectToAction("Index", "Dashboard");
                }

                ModelState.AddModelError(string.Empty, "Failed to login");
            }
            return View(model);

        }


        public async Task<IActionResult> Logout()
        {
            await _userHelper.LogoutAsync();
            return RedirectToAction("Index", "Home");
        }


        public IActionResult Register()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Register(RegisterNewUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userHelper.GetUserByEmailAsync(model.Username);
                if (user == null)
                {
                    user = new User
                    {
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        Email = model.Username,
                        UserName = model.Username
                    };

                    var result = await _userHelper.AddUserAsync(user, model.Password);
                    if (result != IdentityResult.Success)
                    {
                        ModelState.AddModelError(string.Empty, "The user couldn't be created.");
                        return View(model);
                    }


                    await _userHelper.CheckRoleAsync(UserRoles.Customer);
                    await _userHelper.AddUserToRoleAsync(user, UserRoles.Customer);

                    await _customerRepository.CreateAsync(new CustomerProfile
                    {
                        UserId = user.Id
                    });

                    string myToken = await _userHelper.GenerateEmailConfirmationTokenAsync(user);
                    var scheme = Request?.Scheme ?? "https";
                    var tokenLink = Url.Action("ConfirmEmail", "Account", new
                    {
                        userid = user.Id,
                        token = myToken
                    }, protocol: scheme);


                    ApiResponse response = await _mailHelper.SendEmailAsync(model.Username, "Email confirmation", $"<h1>Email Confirmation</h1>" +
                                                       $"Please confirm your email by clicking this link:</br></br><a href = \"{tokenLink}\">Confirm Email</a>");

                    if (response.IsSuccess)
                    {
                        ViewBag.Message = "The instructions to allow you user has been sent to email";
                        return View(model);
                    }

                    ModelState.AddModelError(string.Empty, "The user couldn't be logged.");

                }
            }

            return View(model);
        }


        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            {
                return new NotFoundViewResult("Error404");
            }

            var user = await _userHelper.GetUserByIdAsync(userId);
            if (user == null)
            {
                return new NotFoundViewResult("Error404");
            }

            var result = await _userHelper.ConfirmEmailAsync(user, token);
            if (!result.Succeeded)
            {
                return new NotFoundViewResult("Error404");
            }

            return View();
        }

        public IActionResult ResetPassword(string token, string email)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Error", "Home");
            }

            var model = new ResetPasswordViewModel
            {
                Token = token,
                Username = email
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            var user = await _userHelper.GetUserByEmailAsync(model.Username);
            if (user != null)
            {
                var result = await _userHelper.ResetPasswordAsync(user, model.Token, model.Password);
                if (result.Succeeded)
                {
                    TempData["SuccessMessage"] = "Password reset successful. You can now login to your account.";
                    return RedirectToAction("Login", "Account");
                }

                TempData["FailedMessage"] = "Error while resetting the password.";
                return View(model);
            }

            TempData["NotFoundMessage"] = "User not found.";
            return View(model);
        }


        public IActionResult RecoverPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RecoverPassword(RecoverPasswordViewModel model)
        {
            if (this.ModelState.IsValid)
            {
                var user = await _userHelper.GetUserByEmailAsync(model.Email);
                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "The email doesn't correspond to a registered user.");
                    return View(model);
                }

                var myToken = await _userHelper.GeneratePasswordResetTokenAsync(user);

                var link = this.Url.Action(
                    "ResetPassword",
                    "Account",
                    new { token = myToken, email = user.Email }, protocol: HttpContext.Request.Scheme);

                ApiResponse response = await _mailHelper.SendEmailAsync(model.Email, "Password Reset", $"<h1>Password Reset</h1>" +
                $"To reset the password click in this link:<br/><br>" +
                $"<a href = \"{link}\">Reset Password</a>");

                if (response.IsSuccess)
                {
                    ViewBag.Message = "The instructions to recover your password has been sent to email.";
                }

                return View();
            }

            return View(model);
        }

        [Authorize]
        public async Task<IActionResult> ChangePassword()
        {
            var user = await _userHelper.GetUserByEmailAsync(User.Identity.Name);
            if (user == null) return new NotFoundViewResult("Error404");

            var roles = await _userHelper.GetUserRolesAsync(user);
            ViewData["Role"] = roles.FirstOrDefault();

            return View();
        }

        
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userHelper.GetUserByEmailAsync(User.Identity.Name);
                if (user != null)
                {
                    var roles = await _userHelper.GetUserRolesAsync(user);
                    ViewData["Role"] = roles.FirstOrDefault();

                    var result = await _userHelper.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("EditProfile");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, result.Errors.FirstOrDefault().Description);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "User not found.");
                }
            }
            return View(model);
        }


        [Authorize]
        public async Task<IActionResult> EditProfile()
        {
            var user = await _userHelper.GetUserByEmailAsync(User.Identity.Name);
            if (user == null) return new NotFoundViewResult("Error404");

            var roles = await _userHelper.GetUserRolesAsync(user);
            ViewData["Role"] = roles.FirstOrDefault();

            var model = new EditUserProfileViewModel();

            model.FirstName = user.FirstName;
            model.LastName = user.LastName;
            model.CurrentProfileImagePath = user.ImageFullPath;

            return View(model);
        }

        
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> EditProfile(EditUserProfileViewModel model)
        {
            var user = await _userHelper.GetUserByEmailAsync(User.Identity.Name);
            if (user == null) return new NotFoundViewResult("Error404");


            var roles = await _userHelper.GetUserRolesAsync(user);
            ViewData["Role"] = roles.FirstOrDefault();

            if (!ModelState.IsValid)
            {
                model.CurrentProfileImagePath = user.ImageFullPath;
                return View(model);
            }

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;

            if (model.ImageFile != null && model.ImageFile.Length > 0)
            {
                
                if (model.ImageFile != null && model.ImageFile.Length > 0)
                {
                    var imageId = await _blobHelper.UploadBlobAsync(model.ImageFile, "images");
                    user.ProfileImageId = imageId;
                }
            }

            var response = await _userHelper.UpdateUserAsync(user);

            if (response.Succeeded)
            {
                TempData["UserMessage"] = "Profile updated successfully!";
                return RedirectToAction(nameof(EditProfile));
            }
            else
            {
                ModelState.AddModelError(string.Empty, response.Errors.FirstOrDefault()?.Description ?? "An error occurred.");
            }

            model.CurrentProfileImagePath = user.ImageFullPath;
            return View(model);
        }


        public IActionResult NotAuthorized()
        {
            return View();
        }
    }
}
