using VitoriaAirlinesWeb.Data.Entities;
using VitoriaAirlinesWeb.Models.ViewModels.Dashboard;

namespace VitoriaAirlinesWeb.Data.Repositories
{
    /// <summary>
    /// Defines the contract for a repository handling Ticket entities.
    /// Extends IGenericRepository for common CRUD operations and adds specific ticket-related queries.
    /// </summary>
    public interface ITicketRepository : IGenericRepository<Ticket>
    {
        /// <summary>
        /// Retrieves all tickets associated with a specific user.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>Task: A collection of Ticket entities for the user.</returns>
        Task<IEnumerable<Ticket>> GetTicketsByUserAsync(string userId);


        /// <summary>
        /// Retrieves all tickets associated with a specific flight.
        /// </summary>
        /// <param name="flightId">The ID of the flight.</param>
        /// <returns>Task: A collection of Ticket entities for the flight.</returns>
        Task<IEnumerable<Ticket>> GetTicketsByFlightAsync(int flightId);


        /// <summary>
        /// Retrieves the history of past flights (tickets) for a specific user.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>Task: A collection of Ticket entities representing past flights.</returns>
        Task<IEnumerable<Ticket>> GetTicketsHistoryByUserAsync(string userId);


        /// <summary>
        /// Retrieves all upcoming tickets for a specific user.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>Task: A collection of Ticket entities representing upcoming flights.</returns>
        Task<IEnumerable<Ticket>> GetUpcomingTicketsByUserAsync(string userId);


        /// <summary>
        /// Retrieves a single ticket by its ID, including comprehensive details of associated entities (Flight, Seat, User).
        /// </summary>
        /// <param name="ticketId">The ID of the ticket.</param>
        /// <returns>Task: The Ticket entity with details, or null if not found.</returns>
        Task<Ticket?> GetTicketWithDetailsAsync(int ticketId);


        /// <summary>
        /// Checks if a specific user has already booked a ticket for a given flight.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="flightId">The ID of the flight.</param>
        /// <returns>Task: True if the user has a ticket for the flight, false otherwise.</returns>
        Task<bool> UserHasTicketForFlightAsync(string userId, int flightId);


        /// <summary>
        /// Counts the total number of tickets in the database.
        /// </summary>
        /// <returns>Task: The total count of tickets.</returns>
        Task<int> CountTicketsAsync();


        /// <summary>
        /// Calculates the total revenue generated from all tickets (sum of PricePaid).
        /// </summary>
        /// <returns>Task: The total revenue.</returns>
        Task<decimal> GetTotalRevenueAsync();


        /// <summary>
        /// Retrieves ticket sales data for the last 7 days, grouped by day.
        /// </summary>
        /// <returns>Task: A list of TicketSalesByDayViewModel.</returns>
        Task<List<TicketSalesByDayViewModel>> GetTicketSalesLast7DaysAsync();


        /// <summary>
        /// Retrieves a list of top destinations based on ticket sales.
        /// </summary>
        /// <returns>Task: A list of TopDestinationViewModel.</returns>
        Task<List<TopDestinationViewModel>> GetTopDestinationsAsync();


        /// <summary>
        /// Counts the total number of tickets sold in the last 7 days.
        /// </summary>
        /// <returns>Task: The count of tickets sold in the last 7 days.</returns>
        Task<int> CountTicketsLast7DaysAsync();


        /// <summary>
        /// Counts the number of flights a specific user has booked.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>Task: The count of booked flights for the user.</returns>
        Task<int> CountUserBookedFlightsAsync(string userId);


        /// <summary>
        /// Counts the number of flights a specific user has completed.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>Task: The count of completed flights for the user.</returns>
        Task<int> CountUserCompletedFlightsAsync(string userId);


        /// <summary>
        /// Counts the number of flights a specific user has canceled.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>Task: The count of canceled flights for the user.</returns>
        Task<int> CountUserCanceledFlightsAsync(string userId);


        /// <summary>
        /// Calculates the total amount of money a specific user has spent on tickets.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>Task: The total amount spent by the user.</returns>
        Task<decimal> GetUserTotalSpentAsync(string userId);


        /// <summary>
        /// Retrieves a list of upcoming flights for a specific user.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>Task: A list of FlightInfoViewModel for upcoming flights.</returns>
        Task<List<FlightInfoViewModel>> GetUserUpcomingFlightsAsync(string userId);


        /// <summary>
        /// Retrieves a list of past flights for a specific user.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>Task: A list of FlightInfoViewModel for past flights.</returns>
        Task<List<FlightInfoViewModel>> GetUserPastFlightsAsync(string userId);


        /// <summary>
        /// Retrieves the last completed flight for a specific user.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>Task: A FlightInfoViewModel for the last completed flight, or null.</returns>
        Task<FlightInfoViewModel?> GetUserLastCompletedFlightAsync(string userId);


        /// <summary>
        /// Retrieves the next upcoming flight for a specific user.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>Task: A FlightInfoViewModel for the next upcoming flight, or null.</returns>
        Task<FlightInfoViewModel?> GetUserUpcomingFlightAsync(string userId);


        /// <summary>
        /// Checks if a user has any upcoming flights.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>Task: True if the user has upcoming flights, false otherwise.</returns>
        Task<bool> HasUpcomingFlightsAsync(string userId);


        /// <summary>
        /// Retrieves a ticket by its seat ID and flight ID.
        /// </summary>
        /// <param name="seatId">The ID of the seat.</param>
        /// <param name="flightId">The ID of the flight.</param>
        /// <returns>Task: The Ticket entity, or null if not found.</returns>
        Task<Ticket?> GetBySeatAndFlightAsync(int seatId, int flightId);


        Task<IEnumerable<Ticket>> GetCompletedFlightsByUserAsync(string userId);


        Task<IEnumerable<int>> GetFlightIdsForUserAsync(string userId);

        Task<int> CountUserUpcomingFlightsAsync(string userId);
    }
}