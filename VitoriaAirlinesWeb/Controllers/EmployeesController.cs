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
        /// <summary>
        /// Manages employee accounts. Only accessible by users with the Admin role.
        /// </summary>
        private readonly IUserHelper _userHelper;
        private readonly IMailHelper _mailHelper;
        private readonly IConverterHelper _converterHelper;


        /// <summary>
        /// Initializes a new instance of the EmployeesController with necessary helpers.
        /// </summary>
        /// <param name="userHelper">Helper for user-related operations.</param>
        /// <param name="mailHelper">Helper for sending emails.</param>
        /// <param name="converterHelper">Helper for converting between entities and view models.</param>
        public EmployeesController(
            IUserHelper userHelper,
            IMailHelper mailHelper,
            IConverterHelper converterHelper)
        {
            _userHelper = userHelper;
            _mailHelper = mailHelper;
            _converterHelper = converterHelper;
        }


        /// <summary>
        /// Displays a list of all users with the Employee role.
        /// </summary>
        /// <returns>
        /// Task: A view displaying a collection of employee users.
        /// </returns>
        public async Task<IActionResult> Index()
        {
            var employees = await _userHelper.GetUsersInRoleAsync(UserRoles.Employee);

            return View(employees);
        }


        /// <summary>
        /// Displays the form to register a new employee.
        /// </summary>
        /// <returns>
        /// IActionResult: The register employee view.
        /// </returns>
        public IActionResult Register()
        {
            return View();
        }



        /// <summary>
        /// Handles the submission of the new employee registration form.
        /// Creates a new user with the Employee role, sets a temporary password,
        /// confirms their email, and sends a password reset link via email.
        /// </summary>
        /// <param name="model">The registration data for the new employee.</param>
        /// <returns>
        /// Task: Redirects to the Index on success, or returns the view with validation/error messages.
        /// </returns>
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



        /// <summary>
        /// Displays the form to edit an existing employee's profile.
        /// </summary>
        /// <param name="email">The email of the employee to edit.</param>
        /// <returns>
        /// Task: The edit employee view, pre-populated with employee data, or a 404 error if not found.
        /// </returns>
        public async Task<IActionResult> Edit(string email)
        {
            var employee = await _userHelper.GetUserByEmailAsync(email);
            if (employee == null) return new NotFoundViewResult("Error404");

            var model = _converterHelper.ToEditEmployeeViewModel(employee);
            return View(model);
        }


        /// <summary>
        /// Handles the submission of the employee edit form.
        /// Updates the employee's first name and last name.
        /// </summary>
        /// <param name="email">The original email of the employee (used for lookup).</param>
        /// <param name="model">The updated employee data from the form.</param>
        /// <returns>
        /// Task: Redirects to the Index on success, or returns the view with validation errors.
        /// </returns>
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



        /// <summary>
        /// Deactivates an employee's account by changing their role to 'Deactivated'.
        /// </summary>
        /// <param name="id">The ID of the employee user to deactivate.</param>
        /// <returns>
        /// Task: Redirects to the Index with a success message.
        /// </returns>
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
