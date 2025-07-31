using Microsoft.EntityFrameworkCore;
using VitoriaAirlinesWeb.Data.Entities;

namespace VitoriaAirlinesWeb.Data.Repositories
{
    /// <summary>
    /// Provides data access methods for CustomerProfile entities, extending GenericRepository.
    /// </summary>
    public class CustomerProfileRepository : GenericRepository<CustomerProfile>, ICustomerProfileRepository
    {
        private readonly DataContext _context;

        /// <summary>
        /// Initializes a new instance of the CustomerProfileRepository.
        /// </summary>
        /// <param name="context">The data context for database operations.</param>
        public CustomerProfileRepository(DataContext context) : base(context)
        {
            _context = context;
        }


        /// <summary>
        /// Retrieves a customer profile by the associated user's ID.
        /// Includes the related User and Country entities.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>
        /// Task: The CustomerProfile entity, or null if not found.
        /// </returns>
        public async Task<CustomerProfile?> GetByUserIdAsync(string userId)
        {
            return await _context.CustomerProfiles
                .Include(cp => cp.User)
                .Include(cp => cp.Country)
                .FirstOrDefaultAsync(cp => cp.UserId == userId);
        }


        /// <summary>
        /// Retrieves a customer profile by their passport number.
        /// </summary>
        /// <param name="passportNumber">The passport number to search for.</param>
        /// <returns>
        /// Task: The CustomerProfile entity, or null if not found.
        /// </returns>
        public async Task<CustomerProfile?> GetByPassportAsync(string passportNumber)
        {
            return await _context.CustomerProfiles
                .FirstOrDefaultAsync(cp => cp.PassportNumber == passportNumber);
        }


        /// <summary>
        /// Retrieves a customer profile by its ID, including the related User and Country entities.
        /// </summary>
        /// <param name="id">The ID of the customer profile.</param>
        /// <returns>
        /// Task: The CustomerProfile entity with User and Country, or null if not found.
        /// </returns>
        public async Task<CustomerProfile?> GetByIdWithUserAsync(int id)
        {

            return await _context.CustomerProfiles
                .Include(cp => cp.User)
                .Include(cp => cp.Country)
                .FirstOrDefaultAsync(cp => cp.Id == id);

        }


        /// <summary>
        /// Retrieves a customer profile by its ID, including related User, Tickets, Flights, and Airport countries.
        /// This provides a comprehensive view of the customer's profile and flight history.
        /// </summary>
        /// <param name="id">The ID of the customer profile.</param>
        /// <returns>
        /// Task: The CustomerProfile entity with extensive related data, or null if not found.
        /// </returns>
        public async Task<CustomerProfile?> GetProfileWithUserAndFlightsAsync(int id)
        {
            return await _context.CustomerProfiles
                .Include(cp => cp.User)
                    .ThenInclude(u => u.Tickets)
                        .ThenInclude(t => t.Flight)
                            .ThenInclude(f => f.OriginAirport)
                                .ThenInclude(a => a.Country)
                .Include(cp => cp.User)
                    .ThenInclude(u => u.Tickets)
                        .ThenInclude(t => t.Flight)
                            .ThenInclude(f => f.DestinationAirport)
                                .ThenInclude(a => a.Country)
                .Include(cp => cp.Country)
                .FirstOrDefaultAsync(cp => cp.Id == id);
        }


    }
}
