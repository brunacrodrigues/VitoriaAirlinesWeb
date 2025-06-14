using VitoriaAirlinesWeb.Data.Entities;

namespace VitoriaAirlinesWeb.Helpers
{
    public interface ISeatGeneratorHelper
    {
        List<Seat> GenerateSeats(int airplaneId, int totalExecutiveSeats, int totalEconomySeats);
    }
}
