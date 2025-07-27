using VitoriaAirlinesWeb.Data.Entities;
using VitoriaAirlinesWeb.Models.ViewModels.Dashboard;

namespace VitoriaAirlinesWeb.Data.Repositories
{
    public interface ITicketRepository : IGenericRepository<Ticket>
    {
        Task<IEnumerable<Ticket>> GetTicketsByUserAsync(string userId);

        Task<IEnumerable<Ticket>> GetTicketsByFlightAsync(int flightId);

        Task<IEnumerable<Ticket>> GetTicketsHistoryByUserAsync(string userId);

        Task<IEnumerable<Ticket>> GetUpcomingTicketsByUserAsync(string userId);

        Task<Ticket?> GetTicketWithDetailsAsync(int ticketId);

        Task<bool> UserHasTicketForFlightAsync(string userId, int flightId);

        Task<int> CountTicketsAsync();

        Task<decimal> GetTotalRevenueAsync();

        Task<List<TicketSalesByDayViewModel>> GetTicketSalesLast7DaysAsync();


        Task<List<TopDestinationViewModel>> GetTopDestinationsAsync();

        Task<int> CountTicketsLast7DaysAsync();

        Task<int> CountUserBookedFlightsAsync(string userId);
        Task<int> CountUserCompletedFlightsAsync(string userId);
        Task<int> CountUserCanceledFlightsAsync(string userId);
        Task<decimal> GetUserTotalSpentAsync(string userId);
        Task<List<FlightInfoViewModel>> GetUserUpcomingFlightsAsync(string userId);
        Task<List<FlightInfoViewModel>> GetUserPastFlightsAsync(string userId);

        Task<FlightInfoViewModel?> GetUserLastCompletedFlightAsync(string userId);

        Task<FlightInfoViewModel?> GetUserUpcomingFlightAsync(string userId);

    }
}
