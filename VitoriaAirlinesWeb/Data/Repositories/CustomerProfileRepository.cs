using Microsoft.EntityFrameworkCore;
using VitoriaAirlinesWeb.Data.Entities;

namespace VitoriaAirlinesWeb.Data.Repositories
{
    public class CustomerProfileRepository : GenericRepository<CustomerProfile>, ICustomerProfileRepository
    {
        private readonly DataContext _context;

        public CustomerProfileRepository(DataContext context) : base(context)
        {
            _context = context;
        }


        // gets customer profile of the currently logged in customer
        public async Task<CustomerProfile?> GetByUserIdAsync(string userId)
        {
            return await _context.CustomerProfiles
                .Include(cp => cp.User)
                .Include(cp => cp.Country)
                .FirstOrDefaultAsync(cp => cp.UserId == userId);
        }


        public async Task<CustomerProfile?> GetByPassportAsync(string passportNumber)
        {
            return await _context.CustomerProfiles
                .FirstOrDefaultAsync(cp => cp.PassportNumber == passportNumber);
        }


        // gets a customer profile by its ID including the related User
        public async Task<CustomerProfile?> GetByIdWithUserAsync(int id)
        {

            return await _context.CustomerProfiles
                .Include(cp => cp.User)
                .Include(cp => cp.Country)
                .FirstOrDefaultAsync(cp => cp.Id == id);

        }


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
