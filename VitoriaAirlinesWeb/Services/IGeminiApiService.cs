using VitoriaAirlinesWeb.Models.Dtos;
using VitoriaAirlinesWeb.Responses;

namespace VitoriaAirlinesWeb.Services
{
    /// <summary>
    /// Defines the contract for a service that interacts with the Google Gemini API.
    /// </summary>
    public interface IGeminiApiService
    {
        /// <summary>
        /// Asynchronously sends a prompt to the Google Gemini API and retrieves a response.
        /// </summary>
        /// <param name="prompt">The user's current prompt.</param>
        /// <param name="history">The conversation history.</param>
        /// <param name="userRole">The role of the user, for context.</param>
        /// <returns>Task: An ApiResponse containing the AI's response or an error.</returns>
        Task<ApiResponse> AskAsync(string prompt, IEnumerable<ChatMessageDto> history, string? userRole);
    }
}