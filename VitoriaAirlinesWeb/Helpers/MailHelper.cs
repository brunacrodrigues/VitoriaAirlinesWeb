using MailKit.Net.Smtp;
using MimeKit;
using VitoriaAirlinesWeb.Responses;

namespace VitoriaAirlinesWeb.Helpers
{
    public class MailHelper : IMailHelper
    {
        private readonly IConfiguration _configuration;

        public MailHelper(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        public async Task<ApiResponse> SendEmailAsync(string to, string subject, string body)
        {
            var nameFrom = _configuration["Mail:NameFrom"];
            var from = _configuration["Mail:From"];
            var smtp = _configuration["Mail:SmtpServer"];
            var port = _configuration["Mail:Port"];
            var password = _configuration["Mail:Password"];

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(nameFrom, from));
            message.To.Add(new MailboxAddress(to, to));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = body,
            };
            message.Body = bodyBuilder.ToMessageBody();

            try
            {
                using (var client = new SmtpClient())
                {
                    await client.ConnectAsync(smtp, int.Parse(port), false);
                    await client.AuthenticateAsync(from, password);
                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);
                }
            }
            catch (Exception ex)
            {

                return new ApiResponse
                {
                    IsSuccess = false,
                    Message = ex.ToString()
                };
            }

            return new ApiResponse
            {
                IsSuccess = true,
            };
        }

        public async Task<ApiResponse> SendBookingConfirmationEmailAsync(
        string email,
        string fullName,
        string flightNumber,
        string seatDisplay,
        decimal price,
        DateTime purchaseDateUtc)
        {
            var localPurchaseDate = TimezoneHelper.ConvertToLocal(purchaseDateUtc);

            var body = $@"
        <p>Hello {fullName},</p>
        <p>Thank you for your booking with Vitoria Airlines.</p>
        <p>Your ticket has been successfully issued:</p>
        <ul>
            <li><strong>Flight Number:</strong> {flightNumber}</li>
            <li><strong>Seat:</strong> {seatDisplay}</li>
            <li><strong>Price Paid:</strong> €{price:F2}</li>
            <li><strong>Purchase Date:</strong> {localPurchaseDate:f} (Local Time)</li>
        </ul>
        <p>We wish you a pleasant flight!</p>";

            return await SendEmailAsync(email, "Flight Ticket Confirmation - Vitoria Airlines", body);
        }


    }
}