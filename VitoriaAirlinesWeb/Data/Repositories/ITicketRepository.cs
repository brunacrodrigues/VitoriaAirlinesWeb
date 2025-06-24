using VitoriaAirlinesWeb.Data.Entities;

namespace VitoriaAirlinesWeb.Data.Repositories
{
    public interface ITicketRepository : IGenericRepository<Ticket>
    {
        Task <IEnumerable<Ticket>> GetTicketsByUserAsync(string userId);

        Task<IEnumerable<Ticket>> GetTicketsByFlightAsync(int flightId);
    }
}
