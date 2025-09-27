using VitoriaAirlinesWeb.Data.Entities;
using VitoriaAirlinesWeb.Models.ViewModels.Dashboard;

namespace VitoriaAirlinesWeb.Data.Repositories
{
    /// <summary>
    /// Defines the contract for a repository handling Flight entities.
    /// Extends IGenericRepository for common CRUD operations and adds specific flight-related queries.
    /// </summary>
    public interface IFlightRepository : IGenericRepository<Flight>
    {
        /// <summary>
        /// Retrieves a single flight by its ID, including comprehensive details of related entities.
        /// </summary>
        /// <param name="id">The ID of the flight.</param>
        /// <returns>Task: The Flight entity with details, or null if not found.</returns>
        Task<Flight?> GetByIdWithDetailsAsync(int id);


        /// <summary>
        /// Retrieves all flights including details of associated entities.
        /// </summary>
        /// <returns>IEnumerable: A collection of Flight entities with loaded navigation properties.</returns>
        IEnumerable<Flight> GetAllWithDetailsAsync();


        /// <summary>
        /// Retrieves all scheduled flights that are in the future.
        /// </summary>
        /// <returns>Task: A collection of future scheduled Flight entities.</returns>
        Task<IEnumerable<Flight>> GetScheduledFlightsAsync();


        /// <summary>
        /// Searches for scheduled flights based on optional date, origin, and destination filters.
        /// </summary>
        /// <param name="date">Optional: The departure date.</param>
        /// <param name="originId">Optional: The origin airport ID.</param>
        /// <param name="destinationId">Optional: The destination airport ID.</param>
        /// <returns>Task: A collection of matching Flight entities.</returns>
        Task<IEnumerable<Flight>> SearchFlightsAsync(DateTime? date, int? originId, int? destinationId);


        /// <summary>
        /// Retrieves a history of flights, including completed, departed, and canceled flights.
        /// </summary>
        /// <returns>Task: A collection of past and canceled Flight entities.</returns>
        Task<IEnumerable<Flight>> GetFlightsHistoryAsync();


        /// <summary>
        /// Retrieves a single flight by its ID, including its associated airplane and seats.
        /// </summary>
        /// <param name="id">The ID of the flight.</param>
        /// <returns>Task: The Flight entity with airplane and seat details, or null.</returns>
        Task<Flight?> GetByIdWithAirplaneAndSeatsAsync(int id);


        /// <summary>
        /// Retrieves all flights scheduled for a specific date (UTC).
        /// </summary>
        /// <param name="date">The date to search for flights.</param>
        /// <returns>Task: A collection of flights scheduled for the given date.</returns>
        Task<IEnumerable<Flight>> GetFlightsForDateAsync(DateTime date);


        /// <summary>
        /// Checks if an airplane is available to be assigned to a new flight, considering its existing schedule and maintenance rules.
        /// </summary>
        /// <param name="airplaneId">The ID of the airplane.</param>
        /// <param name="newDepartureUtc">The proposed departure time (UTC) for the new flight.</param>
        /// <param name="newDuration">The proposed duration for the new flight.</param>
        /// <param name="newFlightOriginAirportId">The origin airport ID for the new flight.</param>
        /// <param name="flightToEdit">Optional: The ID of a flight currently being edited.</param>
        /// <returns>Task: True if the airplane is available, false otherwise.</returns>
        Task<bool> IsAirplaneAvailableAsync(
            int airplaneId,
            DateTime newDepartureUtc,
            TimeSpan newDuration,
            int newFlightOriginAirportId,
            int? flightToEdit = null);


        /// <summary>
        /// Retrieves future scheduled flights for a specific airplane that have sold tickets.
        /// </summary>
        /// <param name="airplaneId">The ID of the airplane.</param>
        /// <returns>Task: A collection of future flights with sold tickets.</returns>
        Task<IEnumerable<Flight>> GetFutureFlightsWithSoldTicketsAsync(int airplaneId);


        /// <summary>
        /// Counts the total number of flights in the database.
        /// </summary>
        /// <returns>Task: The total count of flights.</returns>
        Task<int> CountFlightsAsync();


        /// <summary>
        /// Counts the number of scheduled flights.
        /// </summary>
        /// <returns>Task: The count of scheduled flights.</returns>
        Task<int> CountScheduledFlightsAsync();


        /// <summary>
        /// Counts the number of completed flights.
        /// </summary>
        /// <returns>Task: The count of completed flights.</returns>
        Task<int> CountCompletedFlightsAsync();


        /// <summary>
        /// Retrieves a specified number of most recent flights.
        /// </summary>
        /// <param name="count">The maximum number of recent flights to retrieve.</param>
        /// <returns>Task: A list of recent Flight entities.</returns>
        Task<List<Flight>> GetRecentFlightsAsync(int count);


        /// <summary>
        /// Retrieves upcoming scheduled flights with an occupancy rate below a given threshold.
        /// </summary>
        /// <param name="threshold">The occupancy rate percentage.</param>
        /// <param name="maxResults">The maximum number of results to return.</param>
        /// <returns>Task: A list of LowOccupancyFlightViewModel.</returns>
        Task<List<LowOccupancyFlightViewModel>> GetLowOccupancyUpcomingFlightsAsync(double threshold = 50.0, int maxResults = 5);


        /// <summary>
        /// Retrieves all future scheduled flights for a specific airplane.
        /// </summary>
        /// <param name="airplaneId">The ID of the airplane.</param>
        /// <returns>Task: A collection of future flights by airplane.</returns>
        Task<IEnumerable<Flight>> GetFutureFlightsByAirplaneAsync(int airplaneId);


        Task<IEnumerable<Flight>> GetScheduledFlightsByCriteriaAsync(
       int originAirportId,
       int destinationAirportId,
       DateTime date,
       int minPassengers);
    }
}