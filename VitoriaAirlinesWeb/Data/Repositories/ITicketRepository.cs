using VitoriaAirlinesWeb.Data.Entities;

namespace VitoriaAirlinesWeb.Data.Repositories
{
    public interface ITicketRepository : IGenericRepository<Ticket>
    {
        Task <IEnumerable<Ticket>> GetTicketsByUserAsync(string userId);

        Task<IEnumerable<Ticket>> GetTicketsByFlightAsync(int flightId);

        Task<IEnumerable<Ticket>> GetTicketsHistoryByUserAsync(string userId);

        Task<IEnumerable<Ticket>> GetUpcomingTicketsByUserAsync(string userId);

        Task<Ticket?> GetTicketWithDetailsAsync(int ticketId);
    }
}
