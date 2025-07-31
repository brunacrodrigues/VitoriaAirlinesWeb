using VitoriaAirlinesWeb.Responses;

namespace VitoriaAirlinesWeb.Services
{
    /// <summary>
    /// Defines the contract for a service that processes chat prompts.
    /// This interface is typically extended by role-specific prompt services.
    /// </summary>
    public interface IPromptService
    {
        /// <summary>
        /// Asynchronously processes a given prompt string and generates a relevant API response.
        /// </summary>
        /// <param name="prompt">The prompt string provided by the user.</param>
        /// <returns>Task: An ApiResponse containing the message and results of the processed prompt, or null if no action is taken.</returns>
        Task<ApiResponse?> ProcessPromptAsync(string prompt);
    }
}