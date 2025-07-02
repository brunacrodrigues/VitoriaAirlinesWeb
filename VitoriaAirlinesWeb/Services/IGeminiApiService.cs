using VitoriaAirlinesWeb.Models.Dtos;
using VitoriaAirlinesWeb.Responses;

namespace VitoriaAirlinesWeb.Services
{
    public interface IGeminiApiService
    {
        Task<ApiResponse> AskAsync(string prompt, IEnumerable<ChatMessageDto> history, string? userRole);
    }
}
