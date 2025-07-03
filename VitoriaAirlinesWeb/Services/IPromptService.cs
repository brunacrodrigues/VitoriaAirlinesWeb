using VitoriaAirlinesWeb.Responses;

namespace VitoriaAirlinesWeb.Services
{
    public interface IPromptService
    {
        Task<ApiResponse?> ProcessPromptAsync(string prompt);
    }
}
