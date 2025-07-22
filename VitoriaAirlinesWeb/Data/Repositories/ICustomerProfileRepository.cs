using VitoriaAirlinesWeb.Data.Entities;

namespace VitoriaAirlinesWeb.Data.Repositories
{
    public interface ICustomerProfileRepository : IGenericRepository<CustomerProfile>
    {
        Task<CustomerProfile?> GetByUserIdAsync(string userId);

        Task<CustomerProfile?> GetByIdWithUserAsync(int id);

        Task<CustomerProfile?> GetByPassportAsync(string passportNumber);

        Task<CustomerProfile?> GetProfileWithUserAndFlightsAsync(int id);


    }
}
