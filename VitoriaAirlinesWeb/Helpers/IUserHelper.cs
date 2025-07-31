using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using VitoriaAirlinesWeb.Data.Entities;
using VitoriaAirlinesWeb.Models.ViewModels.Account;

namespace VitoriaAirlinesWeb.Helpers
{
    /// <summary>
    /// Defines the contract for a helper class that provides user management functionalities,
    /// integrating with ASP.NET Core Identity.
    /// </summary>
    public interface IUserHelper
    {
        /// <summary>
        /// Asynchronously retrieves a user by their email address.
        /// </summary>
        /// <param name="email">The email address of the user.</param>
        /// <returns>Task: The User entity, or null if not found.</returns>
        Task<User?> GetUserByEmailAsync(string email);


        /// <summary>
        /// Asynchronously adds a new user to the system with the specified password.
        /// </summary>
        /// <param name="user">The User entity to add.</param>
        /// <param name="password">The password for the new user.</param>
        /// <returns>Task: An IdentityResult indicating the success or failure of the operation.</returns>
        Task<IdentityResult> AddUserAsync(User user, string password);


        /// <summary>
        /// Asynchronously logs in a user using provided login credentials.
        /// </summary>
        /// <param name="model">The LoginViewModel containing username and password.</param>
        /// <returns>Task: A SignInResult indicating the outcome of the login attempt.</returns>
        Task<SignInResult> LoginAsync(LoginViewModel model);


        /// <summary>
        /// Asynchronously logs out the current user.
        /// </summary>
        /// <returns>Task: A Task representing the asynchronous logout operation.</returns>
        Task LogoutAsync();


        /// <summary>
        /// Asynchronously updates an existing user's information.
        /// </summary>
        /// <param name="user">The User entity to update.</param>
        /// <returns>Task: An IdentityResult indicating the success or failure of the update operation.</returns>
        Task<IdentityResult> UpdateUserAsync(User user);


        /// <summary>
        /// Asynchronously changes a user's password.
        /// </summary>
        /// <param name="user">The User whose password is to be changed.</param>
        /// <param name="oldPassword">The user's current password.</param>
        /// <param name="newPassword">The new password for the user.</param>
        /// <returns>Task: An IdentityResult indicating the success or failure of the password change.</returns>
        Task<IdentityResult> ChangePasswordAsync(User user, string oldPassword, string newPassword);


        /// <summary>
        /// Asynchronously checks if a role exists, and creates it if it doesn't.
        /// </summary>
        /// <param name="roleName">The name of the role to check/create.</param>
        /// <returns>Task: A Task representing the asynchronous operation.</returns>
        Task CheckRoleAsync(string roleName);


        /// <summary>
        /// Asynchronously adds a user to a specified role.
        /// </summary>
        /// <param name="user">The User to add to the role.</param>
        /// <param name="roleName">The name of the role.</param>
        /// <returns>Task: A Task representing the asynchronous operation.</returns>
        Task AddUserToRoleAsync(User user, string roleName);


        /// <summary>
        /// Asynchronously retrieves the roles associated with a user.
        /// </summary>
        /// <param name="user">The User for whom to retrieve roles.</param>
        /// <returns>Task: A list of strings representing the user's roles.</returns>
        Task<IList<string>> GetUserRolesAsync(User user);


        /// <summary>
        /// Asynchronously checks if a user is in a specific role.
        /// </summary>
        /// <param name="user">The User to check.</param>
        /// <param name="roleName">The name of the role to check against.</param>
        /// <returns>Task: True if the user is in the role, false otherwise.</returns>
        Task<bool> IsUserInRoleAsync(User user, string roleName);


        /// <summary>
        /// Asynchronously validates a user's password.
        /// </summary>
        /// <param name="user">The User whose password is to be validated.</param>
        /// <param name="password">The password to validate.</param>
        /// <returns>Task: A SignInResult indicating the result of the password validation.</returns>
        Task<SignInResult> ValidatePasswordAsync(User user, string password);


        /// <summary>
        /// Asynchronously generates an email confirmation token for a user.
        /// </summary>
        /// <param name="user">The User for whom to generate the token.</param>
        /// <returns>Task: A string representing the email confirmation token.</returns>
        Task<string> GenerateEmailConfirmationTokenAsync(User user);


        /// <summary>
        /// Asynchronously confirms a user's email using a provided token.
        /// </summary>
        /// <param name="user">The User whose email is to be confirmed.</param>
        /// <param name="token">The email confirmation token.</param>
        /// <returns>Task: An IdentityResult indicating the success or failure of the email confirmation.</returns>
        Task<IdentityResult> ConfirmEmailAsync(User user, string token);


        /// <summary>
        /// Asynchronously retrieves a user by their unique ID.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>Task: The User entity, or null if not found.</returns>
        Task<User?> GetUserByIdAsync(string userId);


        /// <summary>
        /// Asynchronously generates a password reset token for a user.
        /// </summary>
        /// <param name="user">The User for whom to generate the token.</param>
        /// <returns>Task: A string representing the password reset token.</returns>
        Task<string> GeneratePasswordResetTokenAsync(User user);


        /// <summary>
        /// Asynchronously resets a user's password using a token.
        /// </summary>
        /// <param name="user">The User whose password is to be reset.</param>
        /// <param name="token">The password reset token.</param>
        /// <param name="password">The new password.</param>
        /// <returns>Task: An IdentityResult indicating the success or failure of the password reset.</returns>
        Task<IdentityResult> ResetPasswordAsync(User user, string token, string password);


        /// <summary>
        /// Asynchronously retrieves a list of users belonging to a specific role.
        /// </summary>
        /// <param name="roleName">The name of the role.</param>
        /// <returns>Task: A list of User entities in the specified role.</returns>
        Task<List<User>> GetUsersInRoleAsync(string roleName);


        /// <summary>
        /// Asynchronously deactivates a user account (e.g., sets a flag or changes status).
        /// Note: The actual deactivation logic needs to be implemented within the helper.
        /// </summary>
        /// <param name="user">The User to deactivate.</param>
        /// <returns>Task: A Task representing the asynchronous deactivation operation.</returns>
        Task DeactivateUserAsync(User user);


        /// <summary>
        /// Asynchronously removes a user from a specific role.
        /// </summary>
        /// <param name="user">The User to remove from the role.</param>
        /// <param name="roleName">The name of the role.</param>
        /// <returns>Task: A Task representing the asynchronous operation.</returns>
        Task RemoveUserFromRole(User user, string roleName);


        /// <summary>
        /// Asynchronously retrieves the User entity associated with the current ClaimsPrincipal.
        /// </summary>
        /// <param name="user">The ClaimsPrincipal representing the current user.</param>
        /// <returns>Task: The User entity, or null if not found.</returns>
        Task<User?> GetUserAsync(ClaimsPrincipal user);


        /// <summary>
        /// Asynchronously counts the number of users in a specific role.
        /// </summary>
        /// <param name="roleName">The name of the role to count users for.</param>
        /// <returns>Task: The total count of users in the specified role.</returns>
        Task<int> CountUsersInRoleAsync(string roleName);
    }
}