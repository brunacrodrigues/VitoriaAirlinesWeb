using System.Text;
using System.Text.Json;
using VitoriaAirlinesWeb.Helpers;
using VitoriaAirlinesWeb.Models.Dtos;
using VitoriaAirlinesWeb.Responses;

namespace VitoriaAirlinesWeb.Services
{
    /// <summary>
    /// Provides services for interacting with the Google Gemini API to generate AI responses.
    /// It handles API calls, request body construction, and response parsing.
    /// </summary>
    public class GeminiApiService : IGeminiApiService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _apiKey;
        private const string GeminiModel = "gemini-2.5-flash"; // Defines the specific Gemini model to use.


        /// <summary>
        /// Initializes a new instance of the GeminiApiService class.
        /// </summary>
        /// <param name="httpClientFactory">Factory for creating HttpClient instances.</param>
        /// <param name="configuration">Application configuration to retrieve the Gemini API key.</param>
        public GeminiApiService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _apiKey = configuration["GoogleGemini:ApiKey"] ?? throw new InvalidOperationException("Google Gemini API Key not configured.");
        }


        /// <summary>
        /// Asynchronously sends a prompt to the Google Gemini API and returns the AI's response.
        /// Includes conversation history and a system instruction based on the user's role.
        /// </summary>
        /// <param name="prompt">The user's current prompt.</param>
        /// <param name="history">A collection of previous chat messages to provide conversation context.</param>
        /// <param name="userRole">The role of the current user (e.g., "Admin", "Customer"), used to tailor AI's behavior.</param>
        /// <returns>
        /// Task: An ApiResponse containing the success status, message, and the AI's generated text result.
        /// Returns IsSuccess = false if there's an HTTP error or parsing issue.
        /// </returns>
        public async Task<ApiResponse> AskAsync(string prompt, IEnumerable<ChatMessageDto> history, string? userRole)
        {
            var apiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/{GeminiModel}:generateContent?key={_apiKey}";
            var requestBody = BuildRequestBody(prompt, history, userRole);
            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var client = _httpClientFactory.CreateClient();

            try
            {
                var response = await client.PostAsync(apiUrl, content); // Sends the POST request to Gemini API.
                response.EnsureSuccessStatusCode(); // Throws HttpRequestException for non-success status codes.
                var responseString = await response.Content.ReadAsStringAsync(); // Reads the response content as a string.

                return ParseResponse(responseString); // Parses the JSON response.
            }
            catch (HttpRequestException httpEx)
            {
                // Handles HTTP-specific errors during the API call.
                return new ApiResponse
                {
                    IsSuccess = false,
                    Message = $"HTTP error calling Google Gemini API: {httpEx.Message}"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse
                {
                    IsSuccess = false,
                    Message = $"Unexpected error: {ex.Message}"
                };
            }
        }



        /// <summary>
        /// Builds the request body for the Gemini API call, including the conversation history
        /// and a system instruction based on the user's role.
        /// </summary>
        /// <param name="prompt">The current user prompt.</param>
        /// <param name="history">The list of previous chat messages.</param>
        /// <param name="userRole">The role of the user, influencing the system instruction.</param>
        /// <returns>An anonymous object representing the JSON request body structure.</returns>
        private object BuildRequestBody(string prompt, IEnumerable<ChatMessageDto> history, string? userRole)
        {
            var systemInstructionText = GetPromptByRole(userRole); // Get the role-specific system instruction.
            var contents = new List<object>();

            // Helper function to add content to the list.
            void AddContent(string role, string text)
                => contents.Add(new { role, parts = new[] { new { text } } });

            // Add historical messages to the contents.
            if (history != null)
            {
                foreach (var msg in history)
                {
                    if (!string.IsNullOrWhiteSpace(msg.Content))
                    {
                        // Map incoming role to Gemini's expected roles ("user" or "model").
                        var role = msg.Role.Equals("user", StringComparison.OrdinalIgnoreCase) ? "user" : "model";
                        AddContent(role, msg.Content);
                    }
                }
            }

            // Add the current user prompt.
            AddContent("user", prompt);

            // Return the final request body object.
            return new
            {
                contents,
                system_instruction = new { parts = new[] { new { text = systemInstructionText } } }
            };
        }


        /// <summary>
        /// Parses the JSON response string from the Gemini API and converts it into an ApiResponse.
        /// Extracts the AI's generated text or identifies blocking reasons.
        /// </summary>
        /// <param name="responseString">The raw JSON response string from the Gemini API.</param>
        /// <returns>An ApiResponse indicating the success, message, and results of the parsing.</returns>
        private ApiResponse ParseResponse(string responseString)
        {
            try
            {
                using var doc = JsonDocument.Parse(responseString);
                var root = doc.RootElement;

                // Check if candidates (AI generated responses) are available.
                if (root.TryGetProperty("candidates", out var candidates) && candidates.GetArrayLength() > 0)
                {
                    var candidate = candidates[0]; // Get the first candidate response.
                    var content = candidate.GetProperty("content");

                    // Extract the text content from the response parts.
                    var text = content.TryGetProperty("parts", out var parts) && parts.GetArrayLength() > 0
                        ? parts[0].GetProperty("text").GetString()
                        : null;

                    return new ApiResponse
                    {
                        IsSuccess = true,
                        Message = "Request successful.",
                        Results = text ?? "Empty response from Gemini."
                    };
                }
                else
                {
                    var blockedReason = "Unknown reason.";
                    if (root.TryGetProperty("promptFeedback", out var feedback) &&
                        feedback.TryGetProperty("blockReason", out var reason))
                    {
                        blockedReason = reason.GetString() ?? blockedReason;
                    }

                    return new ApiResponse
                    {
                        IsSuccess = false,
                        Message = $"Response was blocked by the API. Reason: {blockedReason}"
                    };
                }
            }
            catch (JsonException ex)
            {
                // Handles errors during JSON parsing.
                return new ApiResponse
                {
                    IsSuccess = false,
                    Message = $"Error processing Gemini JSON response: {ex.Message}"
                };
            }
        }


        /// <summary>
        /// Provides a role-specific system instruction string for the Gemini AI.
        /// This guides the AI's persona and response style based on the user's role.
        /// </summary>
        /// <param name="role">The user's role (e.g., "Admin", "Employee", "Customer", or null for anonymous).</param>
        /// <returns>A string containing the system instruction for the AI.</returns>
        private static string GetPromptByRole(string? role)
        {
            return role switch
            {
                UserRoles.Admin => """
            You are a system assistant for administrators of Vitoria Airlines.
            Always respond in English, even if the user writes in another language.
            Provide technical, accurate and concise answers about flights, aircrafts, operational statistics and system usage.
            Keep a professional and objective tone at all times.
            If information is unavailable, say it clearly without inventing data.
        """,

                UserRoles.Employee => """
            You are an internal assistant for Vitoria Airlines employees.
            Always respond in English, even if the user writes in another language.
            Help with tasks such as flight operations, internal procedures, schedules, and shift management.
            Keep your responses brief, clear and practical.
            Do not reveal administrative or financial data unless explicitly allowed.
        """,

                UserRoles.Customer => """
            You are a friendly and helpful assistant for customers of Vitoria Airlines.
            Always respond in English, even if the user writes in another language.
            Assist with information about flights, check-ins, baggage policies, promotions, and travel advice.
            Keep a courteous and accessible tone, focused on customer satisfaction.
            Never provide private data unless the user is authenticated.
        """,

                _ => """
            You are a general-purpose assistant for Vitoria Airlines.
            Always respond in English, regardless of the user's input language.
            Provide helpful and neutral answers about destinations, fleet, services and general travel topics.
            Encourage users to log in or register to receive more personalized support.
        """
            };
        }
    }
}