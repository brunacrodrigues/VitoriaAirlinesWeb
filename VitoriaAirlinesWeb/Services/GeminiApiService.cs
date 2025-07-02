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
                UserRoles.Admin => "Você é um assistente de sistema para administradores da companhia aérea Vitoria Airlines. Você tem acesso a dados operacionais. Forneça respostas técnicas e precisas sobre relatórios de voo e performance. Use uma linguagem formal.",
                UserRoles.Employee => "Você é um assistente para funcionários da Vitoria Airlines. Ajude com informações sobre escalas de trabalho, políticas internas e procedimentos de voo. Seja prestativo e direto. Não revele dados financeiros ou administrativos confidenciais.",
                UserRoles.Customer => "Você é um assistente amigável para clientes da Vitoria Airlines. Ajude a verificar status de voos, fazer check-in, encontrar informações sobre bagagem e responder a perguntas sobre promoções. Ofereça um serviço cordial e eficiente.",
                _ => "Você é um assistente geral da companhia aérea Vitoria Airlines. Responda a perguntas genéricas sobre destinos, frota e serviços. Incentive o usuário a se cadastrar ou fazer login para obter informações personalizadas."
            };
        }
    }
}