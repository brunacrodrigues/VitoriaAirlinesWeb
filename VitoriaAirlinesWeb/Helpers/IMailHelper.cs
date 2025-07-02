using VitoriaAirlinesWeb.Responses;

namespace VitoriaAirlinesWeb.Helpers
{
    public interface IMailHelper
    {
        Task<ApiResponse> SendEmailAsync(string to, string subject, string body);
    }
}
