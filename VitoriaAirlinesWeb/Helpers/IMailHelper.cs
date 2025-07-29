using VitoriaAirlinesWeb.Responses;

namespace VitoriaAirlinesWeb.Helpers
{
    public interface IMailHelper
    {
        Task<ApiResponse> SendEmailAsync(string to, string subject, string body);

        //Task<ApiResponse> SendBookingConfirmationEmailAsync(string email, string fullName, int flightId, int seatId, decimal price, DateTime purchaseDateUtc);

        Task<ApiResponse> SendBookingConfirmationEmailAsync(
    string email,
    string fullName,
    string flightNumber,
    string seatDisplay,
    decimal price,
    DateTime purchaseDateUtc);

    }
}
