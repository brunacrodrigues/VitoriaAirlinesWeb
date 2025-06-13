using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
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

            await CheckCountriesAsync();

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

            await AddCustomerProfilesAsync();
        }

        private async Task CheckCountriesAsync()
        {
            if (!_context.Countries.Any())
            {
                var countriesJson = await File.ReadAllTextAsync("Data/countries.json");

                var countries = JsonSerializer.Deserialize<List<Country>>(countriesJson, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (countries != null)
                {
                    await _context.Countries.AddRangeAsync(countries);
                    await _context.SaveChangesAsync();
                }
            }
        }

        private async Task AddCustomerProfilesAsync()
        {
            var customers = await _userHelper.GetUsersInRoleAsync(UserRoles.Customer);

            foreach (var customer in customers)
            {
                var exists = await _context.CustomerProfiles
                    .AnyAsync(cp => cp.UserId == customer.Id);

                if (!exists)
                {
                    var profile = new CustomerProfile
                    {
                        UserId = customer.Id
                    };

                    await _context.CustomerProfiles.AddAsync(profile);
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}
