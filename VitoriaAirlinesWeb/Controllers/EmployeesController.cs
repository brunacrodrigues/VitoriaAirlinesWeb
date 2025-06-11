using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using VitoriaAirlinesWeb.Data.Entities;
using VitoriaAirlinesWeb.Helpers;
using VitoriaAirlinesWeb.Models;
using VitoriaAirlinesWeb.Responses;

namespace VitoriaAirlinesWeb.Controllers
{
    [Authorize(Roles = UserRoles.Admin)]
    public class EmployeesController : Controller
    {
        private readonly IUserHelper _userHelper;
        private readonly IMailHelper _mailHelper;

        public EmployeesController(
            IUserHelper userHelper,
            IMailHelper mailHelper)
        {
            _userHelper = userHelper;
            _mailHelper = mailHelper;
        }

        public async Task<IActionResult> Index()
        {
            var employees = await _userHelper.GetUsersInRoleAsync(UserRoles.Employee);

            ViewData["Role"] = "Admin";
            return View(employees);
        }

        public IActionResult Register()
        {
            ViewData["Role"] = "Admin";
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Register(RegisterEmployeeViewModel model)
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

                    var tempPassword = "12345678";

                    var result = await _userHelper.AddUserAsync(user, tempPassword);
                    if (result != IdentityResult.Success)
                    {
                        ModelState.AddModelError(string.Empty, "The user couldn't be created.");
                        return View(model);
                    }

                    var emailToken = await _userHelper.GenerateEmailConfirmationTokenAsync(user);
                    await _userHelper.ConfirmEmailAsync(user, emailToken);

                    await _userHelper.CheckRoleAsync(UserRoles.Employee);
                    await _userHelper.AddUserToRoleAsync(user, UserRoles.Employee);

                    var token = await _userHelper.GeneratePasswordResetTokenAsync(user);
                    var scheme = Request?.Scheme ?? "https";
                    var tokenLink = Url.Action("ResetPassword", "Account", new
                    {
                        email = user.Email,
                        token = token
                    }, protocol: scheme);


                    var emailBody = $"<p>Hello {user.FullName},</p>" +
                       $"<p>You have been registered as an employee. Click the link below to change your password:</p>" +
                       $"<p><a href='{tokenLink}'>Change Password</a></p>";

                    Response emailResponse = await _mailHelper.SendEmailAsync(user.Email, "Reset your password", emailBody);

                    if (!emailResponse.IsSuccess)
                    {
                        ModelState.AddModelError(string.Empty, "User created, but email failed.");
                        return View(model);
                    }

                    TempData["SuccessMessage"] = "Employee registered and email sent successfully.";
                    return RedirectToAction(nameof(Index));
                }

            }
            return View(model);
        }


    }
}
