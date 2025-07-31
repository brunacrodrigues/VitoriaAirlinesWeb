namespace VitoriaAirlinesWeb.Data.Repositories
{
    /// <summary>
    /// Defines the contract for a generic repository providing common data access operations.
    /// </summary>
    /// <typeparam name="T">The type of the entity, which must be a class.</typeparam>
    public interface IGenericRepository<T> where T : class
    {
        /// <summary>
        /// Retrieves all entities of type T from the database.
        /// </summary>
        /// <returns>IQueryable: A queryable collection of all entities of type T.</returns>
        IQueryable<T> GetAll();


        /// <summary>
        /// Retrieves a single entity of type T by its ID.
        /// </summary>
        /// <param name="id">The ID of the entity to retrieve.</param>
        /// <returns>Task: The entity of type T, or null if not found.</returns>
        Task<T> GetByIdAsync(int id);


        /// <summary>
        /// Adds a new entity to the database.
        /// </summary>
        /// <param name="entity">The entity to create.</param>
        /// <returns>Task: A Task representing the asynchronous operation.</returns>
        Task CreateAsync(T entity);


        /// <summary>
        /// Updates an existing entity in the database.
        /// </summary>
        /// <param name="entity">The entity to update.</param>
        /// <returns>Task: A Task representing the asynchronous operation.</returns>
        Task UpdateAsync(T entity);


        /// <summary>
        /// Deletes an entity from the database.
        /// </summary>
        /// <param name="entity">The entity to delete.</param>
        /// <returns>Task: A Task representing the asynchronous operation.</returns>
        Task DeleteAsync(T entity);


        /// <summary>
        /// Checks if an entity with the specified ID exists in the database.
        /// </summary>
        /// <param name="id">The ID of the entity to check for existence.</param>
        /// <returns>Task: True if an entity with the given ID exists, false otherwise.</returns>
        Task<bool> ExistsAsync(int id);

    }
}