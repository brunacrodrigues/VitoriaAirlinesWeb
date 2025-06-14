using VitoriaAirlinesWeb.Data.Entities;

namespace VitoriaAirlinesWeb.Data.Repositories
{
    public interface IAirplaneRepository : IGenericRepository<Airplane>
    {
        Task<Airplane?> GetByIdWithSeatsAsync(int id);

        Task AddSeatsAsync(List<Seat> seats);

        Task ReplaceSeatsAsync(int airplaneId, List<Seat> seats);
    }
}
