using VitoriaAirlinesWeb.Data.Entities;
using VitoriaAirlinesWeb.Models.ViewModels.Airplanes;

namespace VitoriaAirlinesWeb.Data.Repositories
{
    public interface IAirplaneRepository : IGenericRepository<Airplane>
    {
        Task<Airplane?> GetByIdWithSeatsAsync(int id);

        Task AddSeatsAsync(List<Seat> seats);

        Task ReplaceSeatsAsync(int airplaneId, List<Seat> seats);

        Task<IEnumerable<AirplaneComboViewModel>> GetComboAirplanesAsync();

        bool CanBeDeleted(int id);

        bool HasAnyFlights(int id);

        bool HasAnyNonCanceledFlights(int id);

        bool HasFutureScheduledFlights(int id);

        IEnumerable<Airplane> GetActiveAirplanes();
    }
}
