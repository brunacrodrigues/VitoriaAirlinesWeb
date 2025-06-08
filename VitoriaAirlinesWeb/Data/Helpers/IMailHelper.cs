using VitoriaAirlinesWeb.Responses;

namespace VitoriaAirlinesWeb.Data.Helpers
{
    public interface IMailHelper
    {
        Task<Response> SendEmailAsync(string to, string subject, string body);
    }
}
