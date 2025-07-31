using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using VitoriaAirlinesWeb.Data.Entities;
using VitoriaAirlinesWeb.Helpers;

namespace VitoriaAirlinesWeb.Data
{
    /// <summary>
    /// Populates the database with initial data (seeding) required for the application.
    /// This includes roles, a default admin user, and customer profiles for existing customer users.
    /// </summary>
    public class SeedDb
    {
        private readonly DataContext _context;
        private readonly IUserHelper _userHelper;


        /// <summary>
        /// Initializes a new instance of the SeedDb class.
        /// </summary>
        /// <param name="context">The data context for database operations.</param>
        /// <param name="userHelper">Helper for user-related operations, including role management.</param>
        public SeedDb(DataContext context, IUserHelper userHelper)
        {
            _context = context;
            _userHelper = userHelper;
        }


        /// <summary>
        /// Asynchronously seeds the database with initial data.
        /// This includes applying migrations, checking and adding countries, roles, a default admin user, and customer profiles.
        /// </summary>
        /// <returns>Task: A Task representing the asynchronous seeding operation.</returns>
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


        /// <summary>
        /// Checks if countries already exist in the database. If not, reads countries from a JSON file
        /// and adds them to the database.
        /// </summary>
        /// <returns>Task: A Task representing the asynchronous operation.</returns>
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


        /// <summary>
        /// Ensures that all users currently in the 'Customer' role have an associated CustomerProfile.
        /// Creates new profiles for any customers missing one.
        /// </summary>
        /// <returns>Task: A Task representing the asynchronous operation.</returns>
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
