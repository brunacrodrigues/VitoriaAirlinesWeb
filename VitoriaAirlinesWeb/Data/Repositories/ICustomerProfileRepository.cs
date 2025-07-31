using VitoriaAirlinesWeb.Data.Entities;

namespace VitoriaAirlinesWeb.Data.Repositories
{
    /// <summary>
    /// Defines the contract for a repository handling CustomerProfile entities.
    /// Extends IGenericRepository for common CRUD operations and adds specific customer profile-related queries.
    /// </summary>
    public interface ICustomerProfileRepository : IGenericRepository<CustomerProfile>
    {
        /// <summary>
        /// Retrieves a customer profile by the associated user's ID.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>Task: The CustomerProfile entity, or null if not found.</returns>
        Task<CustomerProfile?> GetByUserIdAsync(string userId);


        /// <summary>
        /// Retrieves a customer profile by its ID, including the related User.
        /// </summary>
        /// <param name="id">The ID of the customer profile.</param>
        /// <returns>Task: The CustomerProfile entity with User, or null if not found.</returns>
        Task<CustomerProfile?> GetByIdWithUserAsync(int id);


        /// <summary>
        /// Retrieves a customer profile by their passport number.
        /// </summary>
        /// <param name="passportNumber">The passport number to search for.</param>
        /// <returns>Task: The CustomerProfile entity, or null if not found.</returns>
        Task<CustomerProfile?> GetByPassportAsync(string passportNumber);


        /// <summary>
        /// Retrieves a customer profile by its ID, including related User, Tickets, Flights, and Airport countries.
        /// </summary>
        /// <param name="id">The ID of the customer profile.</param>
        /// <returns>Task: The CustomerProfile entity with extensive related data, or null if not found.</returns>
        Task<CustomerProfile?> GetProfileWithUserAndFlightsAsync(int id);


    }
}