using VitoriaAirlinesWeb.Data.Entities;
using VitoriaAirlinesWeb.Models.ViewModels.Airplanes;
using VitoriaAirlinesWeb.Models.ViewModels.Dashboard;

namespace VitoriaAirlinesWeb.Data.Repositories
{
    /// <summary>
    /// Defines the contract for a repository handling Airplane entities.
    /// Extends IGenericRepository for common CRUD operations and adds specific airplane-related queries.
    /// </summary>
    public interface IAirplaneRepository : IGenericRepository<Airplane>
    {
        /// <summary>
        /// Retrieves an airplane by its ID, including its associated seats.
        /// </summary>
        /// <param name="id">The ID of the airplane.</param>
        /// <returns>Task: The Airplane entity with seats, or null if not found.</returns>
        Task<Airplane?> GetByIdWithSeatsAsync(int id);


        /// <summary>
        /// Adds a list of seats to the database.
        /// </summary>
        /// <param name="seats">The list of Seat entities to add.</param>
        /// <returns>Task: A Task representing the asynchronous operation.</returns>
        Task AddSeatsAsync(List<Seat> seats);


        /// <summary>
        /// Replaces existing seats for an airplane with a new list of seats.
        /// </summary>
        /// <param name="airplaneId">The ID of the airplane whose seats are to be replaced.</param>
        /// <param name="seats">The new list of Seat entities.</param>
        /// <returns>Task: A Task representing the asynchronous operation.</returns>
        Task ReplaceSeatsAsync(int airplaneId, List<Seat> seats);


        /// <summary>
        /// Retrieves a list of active airplanes suitable for a combo box.
        /// </summary>
        /// <returns>Task: An enumerable collection of AirplaneComboViewModel.</returns>
        Task<IEnumerable<AirplaneComboViewModel>> GetComboAirplanesAsync();


        /// <summary>
        /// Checks if an airplane can be safely deleted (i.e., has no future scheduled flights).
        /// </summary>
        /// <param name="id">The ID of the airplane.</param>
        /// <returns>True if the airplane can be deleted, false otherwise.</returns>
        bool CanBeDeleted(int id);


        /// <summary>
        /// Checks if an airplane has ever been associated with any flights.
        /// </summary>
        /// <param name="id">The ID of the airplane.</param>
        /// <returns>True if the airplane has any associated flights, false otherwise.</returns>
        bool HasAnyFlights(int id);


        /// <summary>
        /// Checks if an airplane has any flights that are not canceled.
        /// </summary>
        /// <param name="id">The ID of the airplane.</param>
        /// <returns>True if the airplane has any non-canceled flights, false otherwise.</returns>
        bool HasAnyNonCanceledFlights(int id);


        /// <summary>
        /// Checks if an airplane has any future scheduled flights.
        /// </summary>
        /// <param name="id">The ID of the airplane.</param>
        /// <returns>True if the airplane has future scheduled flights, false otherwise.</returns>
        bool HasFutureScheduledFlights(int id);


        /// <summary>
        /// Retrieves all airplanes that are currently in 'Active' status.
        /// </summary>
        /// <returns>IEnumerable: A collection of active Airplane entities.</returns>
        IEnumerable<Airplane> GetActiveAirplanes();


        /// <summary>
        /// Counts the total number of airplanes that are currently 'Active'.
        /// </summary>
        /// <returns>Task: The total count of active airplanes.</returns>
        Task<int> CountAirplanesAsync();


        /// <summary>
        /// Retrieves occupancy statistics for airplanes, including their models and calculated occupancy rates.
        /// </summary>
        /// <returns>Task: A list of AirplaneOccupancyViewModel.</returns>
        Task<List<AirplaneOccupancyViewModel>> GetAirplaneOccupancyStatsAsync();


        /// <summary>
        /// Calculates the average occupancy rate across all non-canceled flights.
        /// </summary>
        /// <returns>Task: The average occupancy rate as a percentage.</returns>
        Task<double> GetAverageOccupancyRateAsync();


        /// <summary>
        /// Retrieves the airplane model with the most associated non-canceled flights.
        /// </summary>
        /// <returns>Task: A MostActiveAirplaneViewModel, or null if no data.</returns>
        Task<MostActiveAirplaneViewModel?> GetMostActiveAirplaneAsync();


        /// <summary>
        /// Retrieves the airplane model with the lowest occupancy rate among non-canceled flights.
        /// </summary>
        /// <returns>Task: A LeastOccupiedAirplaneViewModel, or null if no data.</returns>
        Task<LeastOccupiedAirplaneViewModel?> GetLeastOccupiedModelAsync();
    }
}