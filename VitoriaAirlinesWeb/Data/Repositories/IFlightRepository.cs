using VitoriaAirlinesWeb.Data.Entities;

namespace VitoriaAirlinesWeb.Data.Repositories
{
    public interface IFlightRepository : IGenericRepository<Flight>
    {
        Task<Flight?> GetByIdWithDetailsAsync(int id);

        IEnumerable<Flight> GetAllWithDetailsAsync();

        Task<IEnumerable<Flight>> GetScheduledFlightsAsync();

        Task<IEnumerable<Flight>> SearchFlightsAsync(DateTime? date, int? originId, int? destinationId);

        Task<IEnumerable<Flight>> GetFlightsHistoryAsync();

        Task<Flight?> GetByIdWithAirplaneAndSeatsAsync(int id);

        Task<IEnumerable<Flight>> GetFlightsForDateAsync(DateTime date);
    }
}
