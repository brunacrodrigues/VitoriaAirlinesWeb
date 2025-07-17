using VitoriaAirlinesWeb.Data.Entities;
using VitoriaAirlinesWeb.Models.ViewModels.Dashboard.VitoriaAirlinesWeb.Models.ViewModels.Dashboard;

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

        Task<bool> IsAirplaneAvailableAsync(
            int airplaneId,
            DateTime newDepartureUtc,
            TimeSpan newDuration,
            int newFlightOriginAirportId,
            int? flightToEdit = null);


        Task<IEnumerable<Flight>> GetFutureFlightsWithSoldTicketsAsync(int airplaneId);

        Task<int> CountFlightsAsync();

        Task<int> CountScheduledFlightsAsync();

        Task<int> CountCompletedFlightsAsync();


        Task<List<Flight>> GetRecentFlightsAsync(int count);

        Task<List<LowOccupancyFlightViewModel>> GetLowOccupancyUpcomingFlightsAsync(double threshold = 50.0, int maxResults = 5);
    }
}
