using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VitoriaAirlinesWeb.Data.Entities;
using VitoriaAirlinesWeb.Helpers;

namespace VitoriaAirlinesWeb.Data
{
    public class SeedDb
    {
        private readonly DataContext _context;
        private readonly IUserHelper _userHelper;

        public SeedDb(DataContext context, IUserHelper userHelper)
        {
            _context = context;
            _userHelper = userHelper;
        }

        public async Task SeedAsync()
        {
            await _context.Database.MigrateAsync();

            await _userHelper.CheckRoleAsync(UserRoles.Admin);
            await _userHelper.CheckRoleAsync(UserRoles.Customer);

            var user = await _userHelper.GetUserByEmailAsync("brcrodrigues96@gmail.com");
            if (user == null)
            {
                user = new User
                {
                    FirstName = "Bruna",
                    LastName = "Rodrigues",
                    Email = "brcrodrigues96@gmail.com",
                    UserName = "brcrodrigues96@gmail.com",
                };

                var result = await _userHelper.AddUserAsync(user, "12345678");

                if (result != IdentityResult.Success)
                {
                    throw new InvalidOperationException("Could not create the user in seeder");
                }

                await _userHelper.AddUserToRoleAsync(user, UserRoles.Admin);
                var token = await _userHelper.GenerateEmailConfirmationTokenAsync(user);
                await _userHelper.ConfirmEmailAsync(user, token);
            }

            var isInRole = await _userHelper.IsUserInRoleAsync(user, UserRoles.Admin);
            if (!isInRole)
            {
                await _userHelper.AddUserToRoleAsync(user, UserRoles.Admin);
            }
        }
    }
}
