using VitoriaAirlinesWeb.Data.Entities;

namespace VitoriaAirlinesWeb.Helpers
{
    /// <summary>
    /// Defines the contract for a helper class that generates seat configurations for airplanes.
    /// </summary>
    public interface ISeatGeneratorHelper
    {
        /// <summary>
        /// Generates a list of Seat entities for a given airplane, based on its total executive and economy seats.
        /// </summary>
        /// <param name="airplaneId">The ID of the airplane for which to generate seats.</param>
        /// <param name="totalExecutiveSeats">The total number of executive class seats.</param>
        /// <param name="totalEconomySeats">The total number of economy class seats.</param>
        /// <returns>A List of Seat entities representing the generated seats.</returns>
        List<Seat> GenerateSeats(int airplaneId, int totalExecutiveSeats, int totalEconomySeats);
    }
}