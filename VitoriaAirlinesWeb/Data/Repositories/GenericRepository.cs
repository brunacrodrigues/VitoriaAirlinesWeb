using Microsoft.EntityFrameworkCore;
using VitoriaAirlinesWeb.Data.Entities;

namespace VitoriaAirlinesWeb.Data.Repositories
{
    /// <summary>
    /// Implements a generic repository pattern for common data access operations
    /// on entities that implement the IEntity interface.
    /// </summary>
    /// <typeparam name="T">The type of the entity, which must be a class and implement IEntity.</typeparam>
    public class GenericRepository<T> : IGenericRepository<T> where T : class, IEntity
    {
        private readonly DataContext _context;

        /// <summary>
        /// Initializes a new instance of the GenericRepository.
        /// </summary>
        /// <param name="context">The data context for database operations.</param>
        public GenericRepository(DataContext context)
        {
            _context = context;
        }


        /// <summary>
        /// Retrieves all entities of type T from the database. The query is configured for no tracking.
        /// </summary>
        /// <returns>
        /// An IQueryable of all entities of type T.
        /// </returns>
        public IQueryable<T> GetAll()
        {
            return _context.Set<T>().AsNoTracking();
        }


        /// <summary>
        /// Retrieves a single entity of type T by its ID. The query is configured for no tracking.
        /// </summary>
        /// <param name="id">The ID of the entity to retrieve.</param>
        /// <returns>
        /// Task: The entity of type T, or null if not found.
        /// </returns>
        public async Task<T> GetByIdAsync(int id)
        {
            return await _context.Set<T>().AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);
        }


        /// <summary>
        /// Adds a new entity to the database.
        /// </summary>
        /// <param name="entity">The entity to create.</param>
        /// <returns>
        /// Task: A Task representing the asynchronous operation.
        /// </returns>
        public async Task CreateAsync(T entity)
        {
            await _context.Set<T>().AddAsync(entity);
            await SaveAllAsync();
        }


        /// <summary>
        /// Updates an existing entity in the database.
        /// </summary>
        /// <param name="entity">The entity to update.</param>
        /// <returns>
        /// Task: A Task representing the asynchronous operation.
        /// </returns>
        public async Task UpdateAsync(T entity)
        {
            _context.Set<T>().Update(entity);
            await SaveAllAsync();
        }


        /// <summary>
        /// Deletes an entity from the database.
        /// </summary>
        /// <param name="entity">The entity to delete.</param>
        /// <returns>
        /// Task: A Task representing the asynchronous operation.
        /// </returns>
        public async Task DeleteAsync(T entity)
        {
            _context.Set<T>().Remove(entity);
            await SaveAllAsync();
        }


        /// <summary>
        /// Checks if an entity with the specified ID exists in the database.
        /// </summary>
        /// <param name="id">The ID of the entity to check for existence.</param>
        /// <returns>
        /// Task: True if an entity with the given ID exists, false otherwise.
        /// </returns>
        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Set<T>().AnyAsync(e => e.Id == id);
        }


        // <summary>
        /// Saves all changes made in the context to the database.
        /// </summary>
        /// <returns>
        /// Task: True if any changes were saved (number of state entries written to the database is greater than 0), false otherwise.
        /// </returns>
        private async Task<bool> SaveAllAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
