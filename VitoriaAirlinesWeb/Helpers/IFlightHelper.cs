namespace VitoriaAirlinesWeb.Helpers
{
    /// <summary>
    /// Defines the contract for a helper class that provides flight-related utility functions.
    /// </summary>
    public interface IFlightHelper
    {
        /// <summary>
        /// Asynchronously generates a unique flight number.
        /// </summary>
        /// <returns>Task: A unique flight number string.</returns>
        Task<string> GenerateUniqueFlightNumberAsync();
    }
}