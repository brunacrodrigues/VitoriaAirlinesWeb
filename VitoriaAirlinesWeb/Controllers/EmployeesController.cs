using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using VitoriaAirlinesWeb.Data.Entities;
using VitoriaAirlinesWeb.Helpers;
using VitoriaAirlinesWeb.Models.ViewModels.Employees;
using VitoriaAirlinesWeb.Responses;

namespace VitoriaAirlinesWeb.Controllers
{
    [Authorize(Roles = UserRoles.Admin)]
    public class EmployeesController : Controller
    {
        private readonly IUserHelper _userHelper;
        private readonly IMailHelper _mailHelper;
        private readonly IConverterHelper _converterHelper;

        public EmployeesController(
            IUserHelper userHelper,
            IMailHelper mailHelper,
            IConverterHelper converterHelper)
        {
            _userHelper = userHelper;
            _mailHelper = mailHelper;
            _converterHelper = converterHelper;
        }

        public async Task<IActionResult> Index()
        {
            var employees = await _userHelper.GetUsersInRoleAsync(UserRoles.Employee);

            return View(employees);
        }

        public IActionResult Register()
        {
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

                    ApiResponse emailResponse = await _mailHelper.SendEmailAsync(user.Email, "Reset your password", emailBody);

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


        public async Task<IActionResult> Edit(string email)
        {
            var employee = await _userHelper.GetUserByEmailAsync(email);
            if (employee == null) return new NotFoundViewResult("Error404");

            var model = _converterHelper.ToEditEmployeeViewModel(employee);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string email, EditEmployeeViewModel model)
        {

            if (email != model.Email) return new NotFoundViewResult("Error404");

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var employee = await _userHelper.GetUserByEmailAsync(email);
            if (employee == null) return new NotFoundViewResult("Error404");

            _converterHelper.ToUser(model, employee);
            await _userHelper.UpdateUserAsync(employee);

            TempData["SuccessMessage"] = "Employee updated successfully.";
            return RedirectToAction(nameof(Index));
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var employee = await _userHelper.GetUserByIdAsync(id);
            if (employee == null) return new NotFoundViewResult("Error404");

            await _userHelper.DeactivateUserAsync(employee);
            await _userHelper.RemoveUserFromRole(employee, UserRoles.Employee);

            await _userHelper.CheckRoleAsync(UserRoles.Deactivated);
            await _userHelper.AddUserToRoleAsync(employee, UserRoles.Deactivated);

            TempData["SuccessMessage"] = "Employee deactivated successfully.";
            return RedirectToAction(nameof(Index));
        }


    }
}
