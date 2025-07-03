using System.Text;
using System.Text.Json;
using VitoriaAirlinesWeb.Helpers;
using VitoriaAirlinesWeb.Models.Dtos;
using VitoriaAirlinesWeb.Responses;

namespace VitoriaAirlinesWeb.Services
{
    public class GeminiApiService : IGeminiApiService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _apiKey;
        private const string GeminiModel = "gemini-2.5-flash";

        public GeminiApiService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _apiKey = configuration["GoogleGemini:ApiKey"];
        }


        public async Task<ApiResponse> AskAsync(string prompt, IEnumerable<ChatMessageDto> history, string? userRole)
        {
            var apiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/{GeminiModel}:generateContent?key={_apiKey}";
            var requestBody = BuildRequestBody(prompt, history, userRole);
            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var client = _httpClientFactory.CreateClient();

            try
            {
                var response = await client.PostAsync(apiUrl, content);
                response.EnsureSuccessStatusCode();
                var responseString = await response.Content.ReadAsStringAsync();

                return ParseResponse(responseString);
            }
            catch (HttpRequestException httpEx)
            {
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

        private object BuildRequestBody(string prompt, IEnumerable<ChatMessageDto> history, string? userRole)
        {
            var systemInstructionText = GetPromptByRole(userRole);
            var contents = new List<object>();

            void AddContent(string role, string text)
                => contents.Add(new { role, parts = new[] { new { text } } });

            if (history != null)
            {
                foreach (var msg in history)
                {
                    if (!string.IsNullOrWhiteSpace(msg.Content))
                    {
                        var role = msg.Role.Equals("user", StringComparison.OrdinalIgnoreCase) ? "user" : "model";
                        AddContent(role, msg.Content);
                    }
                }
            }

            AddContent("user", prompt);

            return new
            {
                contents,
                system_instruction = new { parts = new[] { new { text = systemInstructionText } } }
            };
        }


        private ApiResponse ParseResponse(string responseString)
        {
            try
            {
                using var doc = JsonDocument.Parse(responseString);
                var root = doc.RootElement;

                if (root.TryGetProperty("candidates", out var candidates) && candidates.GetArrayLength() > 0)
                {
                    var candidate = candidates[0];
                    var content = candidate.GetProperty("content");

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
                return new ApiResponse
                {
                    IsSuccess = false,
                    Message = $"Error processing Gemini JSON response: {ex.Message}"
                };
            }
        }


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