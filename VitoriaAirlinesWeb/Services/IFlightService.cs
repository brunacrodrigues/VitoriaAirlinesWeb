namespace VitoriaAirlinesWeb.Services
{
    /// <summary>
    /// Defines the contract for a service that manages flight-related background tasks.
    /// </summary>
    public interface IFlightService
    {
        /// <summary>
        /// Asynchronously updates the status of flights based on their scheduled times.
        /// </summary>
        /// <returns>Task: A Task representing the asynchronous operation.</returns>
        Task UpdateFlightStatusAsync();
    }
}