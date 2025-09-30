using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using VitoriaAirlinesWeb.Data.Entities;
using VitoriaAirlinesWeb.Data.Repositories;
using VitoriaAirlinesWeb.Helpers;
using VitoriaAirlinesWeb.Models.ViewModels.Account;
using VitoriaAirlinesWeb.Responses;

namespace VitoriaAirlinesWeb.Controllers
{
    /// <summary>
    /// Handles all user account-related actions such as login, registration, password management,
    /// email confirmation, and profile updates.
    /// </summary>
    public class AccountController : Controller
    {
        private readonly IUserHelper _userHelper;
        private readonly IMailHelper _mailHelper;
        private readonly IBlobHelper _blobHelper;
        private readonly ICustomerProfileRepository _customerRepository;


        /// <summary>
        /// Initializes a new instance of the <see cref="AccountController"/> class with necessary
        /// helper services and repositories for user management, email communication, blob storage,
        /// and customer profile operations.
        /// </summary>
        /// <param name="userHelper">Helper for user-related operations, including authentication and authorization.</param>
        /// <param name="mailHelper">Helper for sending emails, such as confirmation and password reset emails.</param>
        /// <param name="blobHelper">Helper for interacting with blob storage, specifically for user profile images.</param>
        /// <param name="customerRepository">Repository for managing customer profile data.</param>
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


        /// <summary>
        /// Displays the login form.
        /// </summary>
        /// <returns>
        /// IActionResult: The login view (ViewResult).
        /// </returns>
        public IActionResult Login()
        {

            return View();
        }


        /// <summary>
        /// Handles user login submission.
        /// </summary>
        /// <param name="model">Login credentials.</param>
        /// <returns>
        /// Task: Redirects on success (Dashboard or ReturnUrl), or returns view with errors on failure.
        /// </returns>
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


        /// <summary>
        /// Logs out the current user.
        /// </summary>
        /// <returns>
        /// Task: Redirects to Home/Index (RedirectToActionResult).
        /// </returns>
        public async Task<IActionResult> Logout()
        {
            await _userHelper.LogoutAsync();
            return RedirectToAction("Index", "Home");
        }


        /// <summary>
        /// Displays the registration form.
        /// </summary>
        /// <returns>
        /// IActionResult: The registration view (ViewResult).
        /// </returns>
        public IActionResult Register()
        {
            return View();
        }



        /// <summary>
        /// Handles new user registration and sends a confirmation email.
        /// </summary>
        /// <param name="model">Registration data.</param>
        /// <returns>
        /// Task: Returns view with success message or errors.
        /// </returns>
        [HttpPost]
        public async Task<IActionResult> Register(RegisterNewUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var existingUser = await _userHelper.GetUserByEmailAsync(model.Username);
                if (existingUser != null)
                {
                    ModelState.AddModelError(string.Empty, "The user couldn't be registered.");
                    return View(model);
                }

                var user = new User
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Username,
                    UserName = model.Username
                };

                var result = await _userHelper.AddUserAsync(user, model.Password);
                if (!result.Succeeded)
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


                var tokenBytes = Encoding.UTF8.GetBytes(myToken);
                var base64Token = WebEncoders.Base64UrlEncode(tokenBytes);

                var scheme = Request?.Scheme ?? "https";
                var tokenLink = Url.Action("ConfirmEmail", "Account", new
                {
                    userid = user.Id,
                    token = base64Token
                }, protocol: scheme);

                ApiResponse response = await _mailHelper.SendEmailAsync(
                    model.Username,
                    "Email confirmation",
                    $"<h1>Email Confirmation</h1>" +
                    $"Please confirm your email by clicking this link:</br></br>" +
                    $"<a href=\"{tokenLink}\">Confirm Email</a>");

                if (response.IsSuccess)
                {
                    ViewBag.Message = "The instructions to allow your user have been sent to your email.";
                    return View(model);
                }

                ModelState.AddModelError(string.Empty, "The user couldn't be registered.");
            }

            return View(model);
        }



        /// <summary>
        /// Confirms a user's email address.
        /// </summary>
        /// <param name="userId">User identifier.</param>
        /// <param name="token">Email confirmation token.</param>
        /// <returns>
        /// Task: Returns the confirmation view on success, or a 404 error view on failure.
        /// </returns>        
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

            string decodedToken;

            try
            {
                var tokenBytes = WebEncoders.Base64UrlDecode(token);
                decodedToken = Encoding.UTF8.GetString(tokenBytes);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"Token decoding failed: {ex.Message}";
                ViewBag.UserId = userId;
                ViewBag.Token = token;
                return View("ConfirmEmailError");
            }

            var result = await _userHelper.ConfirmEmailAsync(user, decodedToken);

            if (!result.Succeeded)
            {
                ViewBag.ErrorMessage = string.Join(" | ", result.Errors.Select(e => $"{e.Code}: {e.Description}"));
                ViewBag.UserId = userId;
                ViewBag.Token = token;
                ViewBag.DecodedToken = decodedToken;
                return View("ConfirmEmailError");
            }

            return View();
        }


        /// <summary>
        /// Displays the reset password form.
        /// </summary>
        /// <param name="token">Password reset token.</param>
        /// <param name="email">User's email.</param>
        /// <returns>
        /// IActionResult: Returns the reset password view with token/email, or redirects to error page if parameters are invalid.
        /// </returns>
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


        /// <summary>
        /// Handles reset password submission.
        /// </summary>
        /// <param name="model">Reset password data.</param>
        /// <returns>
        /// Task: Redirects to Login on success, or returns view with errors.
        /// </returns>      
        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userHelper.GetUserByEmailAsync(model.Username);
            if (user != null)
            {
                var tokenBytes = WebEncoders.Base64UrlDecode(model.Token);
                var decodedToken = Encoding.UTF8.GetString(tokenBytes);

                var result = await _userHelper.ResetPasswordAsync(user, decodedToken, model.Password);
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



        /// <summary>
        /// Displays the recover password form.
        /// </summary>
        /// <returns>
        /// IActionResult: The recover password view (ViewResult).
        /// </returns>
        public IActionResult RecoverPassword()
        {
            return View();
        }


        /// <summary>
        /// Handles recover password submission.
        /// </summary>
        /// <param name="model">Email for password recovery.</param>
        /// <returns>
        /// Task: Sends reset email if user exists, returns view with messages.
        /// </returns>       
        [HttpPost]
        public async Task<IActionResult> RecoverPassword(RecoverPasswordViewModel model)
        {
            if (this.ModelState.IsValid)
            {
                var user = await _userHelper.GetUserByEmailAsync(model.Email);
                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "Invalid email.");
                    return View(model);
                }

                var myToken = await _userHelper.GeneratePasswordResetTokenAsync(user);


                var tokenBytes = Encoding.UTF8.GetBytes(myToken);
                var base64Token = WebEncoders.Base64UrlEncode(tokenBytes);

                var link = this.Url.Action(
                    "ResetPassword",
                    "Account",
                    new { token = base64Token, email = user.Email },
                    protocol: HttpContext.Request.Scheme);

                ApiResponse response = await _mailHelper.SendEmailAsync(
                    model.Email,
                    "Password Reset",
                    $"<h1>Password Reset</h1>" +
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


        /// <summary>
        /// Displays the form to change the current user's password. Requires authorization.
        /// </summary>
        /// <returns>
        /// Task: Returns view with user role, or 404 error if user not found.
        /// </returns>
        [Authorize]
        public async Task<IActionResult> ChangePassword()
        {
            var user = await _userHelper.GetUserByEmailAsync(User.Identity.Name);
            if (user == null) return new NotFoundViewResult("Error404");

            var roles = await _userHelper.GetUserRolesAsync(user);
            ViewData["Role"] = roles.FirstOrDefault();

            return View();
        }



        /// <summary>
        /// Handles change password submission. Requires authorization.
        /// </summary>
        /// <param name="model">Change password data.</param>
        /// <returns>
        /// Task: Redirects on success, or returns view with errors.
        /// </returns>
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

                    var result = await _userHelper.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
                    if (result.Succeeded)
                    {
                        TempData["SuccessMessage"] = "Password changed successfully.";

                        if (roles.Contains(UserRoles.Customer))
                        {
                            return RedirectToAction("EditTravellerProfile", "Customers");
                        }

                        return RedirectToAction("EditProfile", "Account");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, result.Errors.FirstOrDefault()?.Description ?? "Password change failed.");
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "User not found.");
                }
            }

            return View(model);
        }


        /// <summary>
        /// Displays the form for the current user to edit their profile. Requires authorization.
        /// </summary>
        /// <returns>
        /// Task: Returns view with profile data, or 404 error if user not found.
        /// </returns>
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



        /// <summary>
        /// Handles edit profile submission. Requires authorization.
        /// </summary>
        /// <param name="model">Edit profile data.</param>
        /// <returns>
        /// Task: Redirects to self on success, or returns view with errors.
        /// </returns>
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

            if (Request.Form["RemoveImage"] == "true")
            {
                user.ProfileImageId = null;
            }
            else if (model.ImageFile != null && model.ImageFile.Length > 0)
            {
                var imageId = await _blobHelper.UploadBlobAsync(model.ImageFile, "images");
                user.ProfileImageId = imageId;
            }

            var response = await _userHelper.UpdateUserAsync(user);

            if (response.Succeeded)
            {
                TempData["SuccessMessage"] = "Profile updated successfully!";
                return RedirectToAction(nameof(EditProfile));
            }
            else
            {
                ModelState.AddModelError(string.Empty, response.Errors.FirstOrDefault()?.Description ?? "An error occurred.");
            }

            model.CurrentProfileImagePath = user.ImageFullPath;
            return View(model);
        }


        /// <summary>
        /// Displays a view indicating unauthorized access.
        /// </summary>
        /// <returns>
        /// IActionResult: The "NotAuthorized" view (ViewResult).
        /// </returns>
        public IActionResult NotAuthorized()
        {
            return View();
        }
    }
}
