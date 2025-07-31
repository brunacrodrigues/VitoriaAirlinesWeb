using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using VitoriaAirlinesWeb.Data.Entities;
using VitoriaAirlinesWeb.Models.ViewModels.Account;

namespace VitoriaAirlinesWeb.Helpers
{
    /// <summary>
    /// Provides helper methods for managing user accounts, roles, and authentication
    /// by wrapping ASP.NET Core Identity functionalities.
    /// </summary>
    public class UserHelper : IUserHelper
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        /// <summary>
        /// Initializes a new instance of the UserHelper class.
        /// </summary>
        /// <param name="userManager">The UserManager for user-related operations.</param>
        /// <param name="signInManager">The SignInManager for user sign-in/sign-out operations.</param>
        /// <param name="roleManager">The RoleManager for role-related operations.</param>
        public UserHelper(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }


        /// <summary>
        /// Asynchronously adds a new user to the system with the specified password.
        /// </summary>
        /// <param name="user">The User entity to add.</param>
        /// <param name="password">The password for the new user.</param>
        /// <returns>Task: An IdentityResult indicating the success or failure of the operation.</returns>
        public async Task<IdentityResult> AddUserAsync(User user, string password)
        {
            return await _userManager.CreateAsync(user, password);
        }


        /// <summary>
        /// Asynchronously adds a user to a specified role.
        /// </summary>
        /// <param name="user">The User to add to the role.</param>
        /// <param name="roleName">The name of the role.</param>
        /// <returns>Task: A Task representing the asynchronous operation.</returns>
        public async Task AddUserToRoleAsync(User user, string roleName)
        {
            await _userManager.AddToRoleAsync(user, roleName);
        }



        /// <summary>
        /// Asynchronously changes a user's password.
        /// </summary>
        /// <param name="user">The User whose password is to be changed.</param>
        /// <param name="oldPassword">The user's current password.</param>
        /// <param name="newPassword">The new password for the user.</param>
        /// <returns>Task: An IdentityResult indicating the success or failure of the password change.</returns>
        public async Task<IdentityResult> ChangePasswordAsync(User user, string oldPassword, string newPassword)
        {
            return await _userManager.ChangePasswordAsync(user, oldPassword, newPassword);
        }



        /// <summary>
        /// Asynchronously checks if a role exists, and creates it if it doesn't.
        /// </summary>
        /// <param name="roleName">The name of the role to check/create.</param>
        /// <returns>Task: A Task representing the asynchronous operation.</returns>
        public async Task CheckRoleAsync(string roleName)
        {
            var roleExists = await _roleManager.RoleExistsAsync(roleName);

            if (!roleExists)
            {
                await _roleManager.CreateAsync(new IdentityRole
                {
                    Name = roleName
                });
            }
        }


        /// <summary>
        /// Asynchronously confirms a user's email using a provided token.
        /// </summary>
        /// <param name="user">The User whose email is to be confirmed.</param>
        /// <param name="token">The email confirmation token.</param>
        /// <returns>Task: An IdentityResult indicating the success or failure of the email confirmation.</returns>
        public async Task<IdentityResult> ConfirmEmailAsync(User user, string token)
        {
            return await _userManager.ConfirmEmailAsync(user, token);
        }


        /// <summary>
        /// Asynchronously generates an email confirmation token for a user.
        /// </summary>
        /// <param name="user">The User for whom to generate the token.</param>
        /// <returns>Task: A string representing the email confirmation token.</returns>
        public async Task<string> GenerateEmailConfirmationTokenAsync(User user)
        {
            return await _userManager.GenerateEmailConfirmationTokenAsync(user);
        }


        /// <summary>
        /// Asynchronously generates a password reset token for a user.
        /// </summary>
        /// <param name="user">The User for whom to generate the token.</param>
        /// <returns>Task: A string representing the password reset token.</returns>
        public async Task<string> GeneratePasswordResetTokenAsync(User user)
        {
            return await _userManager.GeneratePasswordResetTokenAsync(user);
        }


        /// <summary>
        /// Asynchronously retrieves the roles associated with a user.
        /// </summary>
        /// <param name="user">The User for whom to retrieve roles.</param>
        /// <returns>Task: A list of strings representing the user's roles.</returns>
        public async Task<IList<string>> GetUserRolesAsync(User user)
        {
            return await _userManager.GetRolesAsync(user);
        }


        /// <summary>
        /// Asynchronously retrieves a user by their email address.
        /// </summary>
        /// <param name="email">The email address of the user.</param>
        /// <returns>Task: The User entity, or null if not found.</returns>
        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _userManager.FindByEmailAsync(email);
        }


        /// <summary>
        /// Asynchronously retrieves a user by their unique ID.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>Task: The User entity, or null if not found.</returns>
        public async Task<User?> GetUserByIdAsync(string userId)
        {
            return await _userManager.FindByIdAsync(userId);
        }


        /// <summary>
        /// Asynchronously checks if a user is in a specific role.
        /// </summary>
        /// <param name="user">The User to check.</param>
        /// <param name="roleName">The name of the role to check against.</param>
        /// <returns>Task: True if the user is in the role, false otherwise.</returns>
        public async Task<bool> IsUserInRoleAsync(User user, string roleName)
        {
            return await _userManager.IsInRoleAsync(user, roleName);
        }


        /// <summary>
        /// Asynchronously logs in a user using provided login credentials.
        /// </summary>
        /// <param name="model">The LoginViewModel containing username and password.</param>
        /// <returns>Task: A SignInResult indicating the outcome of the login attempt.</returns>
        public async Task<SignInResult> LoginAsync(LoginViewModel model)
        {
            return await _signInManager.PasswordSignInAsync(
                model.Username,
                model.Password,
                model.RememberMe,
                false); // Last parameter (lockoutOnFailure) is false here, meaning no account lockout on failed login attempts.
        }


        /// <summary>
        /// Asynchronously logs out the current user.
        /// </summary>
        /// <returns>Task: A Task representing the asynchronous logout operation.</returns>
        public async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync();
        }


        /// <summary>
        /// Asynchronously resets a user's password using a token.
        /// </summary>
        /// <param name="user">The User whose password is to be reset.</param>
        /// <param name="token">The password reset token.</param>
        /// <param name="password">The new password.</param>
        /// <returns>Task: An IdentityResult indicating the success or failure of the password reset.</returns>
        public async Task<IdentityResult> ResetPasswordAsync(User user, string token, string password)
        {
            return await _userManager.ResetPasswordAsync(user, token, password);
        }


        /// <summary>
        /// Asynchronously updates an existing user's information.
        /// </summary>
        /// <param name="user">The User entity to update.</param>
        /// <returns>Task: An IdentityResult indicating the success or failure of the update operation.</returns>
        public async Task<IdentityResult> UpdateUserAsync(User user)
        {
            return await _userManager.UpdateAsync(user);
        }


        /// <summary>
        /// Asynchronously validates a user's password.
        /// </summary>
        /// <param name="user">The User whose password is to be validated.</param>
        /// <param name="password">The password to validate.</param>
        /// <returns>Task: A SignInResult indicating the result of the password validation.</returns>
        public async Task<SignInResult> ValidatePasswordAsync(User user, string password)
        {
            return await _signInManager.CheckPasswordSignInAsync(user, password, false); // Last parameter (lockoutOnFailure) is false here.
        }


        /// <summary>
        /// Asynchronously retrieves a list of users belonging to a specific role.
        /// </summary>
        /// <param name="roleName">The name of the role.</param>
        /// <returns>Task: A list of User entities in the specified role.</returns>
        public async Task<List<User>> GetUsersInRoleAsync(string roleName)
        {
            var users = await _userManager.GetUsersInRoleAsync(roleName);
            return users.ToList();
        }


        /// <summary>
        /// Asynchronously deactivates a user account by setting their lockout end date to a maximum value.
        /// This effectively locks the user out indefinitely.
        /// </summary>
        /// <param name="user">The User to deactivate.</param>
        /// <returns>Task: A Task representing the asynchronous deactivation operation.</returns>
        public async Task DeactivateUserAsync(User user)
        {
            await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
        }


        /// <summary>
        /// Asynchronously removes a user from a specific role.
        /// </summary>
        /// <param name="user">The User to remove from the role.</param>
        /// <param name="roleName">The name of the role.</param>
        /// <returns>Task: A Task representing the asynchronous operation.</returns>
        public async Task RemoveUserFromRole(User user, string roleName)
        {
            await _userManager.RemoveFromRoleAsync(user, roleName);
        }


        /// <summary>
        /// Asynchronously retrieves the User entity associated with the current ClaimsPrincipal.
        /// </summary>
        /// <param name="user">The ClaimsPrincipal representing the current user.</param>
        /// <returns>Task: The User entity, or null if not found.</returns>
        public async Task<User?> GetUserAsync(ClaimsPrincipal user)
        {
            return await _userManager.GetUserAsync(user);
        }


        /// <summary>
        /// Asynchronously counts the number of users in a specific role.
        /// </summary>
        /// <param name="roleName">The name of the role to count users for.</param>
        /// <returns>Task: The total count of users in the specified role.</returns>
        public async Task<int> CountUsersInRoleAsync(string roleName)
        {
            var usersInRole = await _userManager.GetUsersInRoleAsync(roleName);
            return usersInRole.Count;
        }
    }
}
