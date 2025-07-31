using MailKit.Net.Smtp;
using MimeKit;
using VitoriaAirlinesWeb.Responses;

namespace VitoriaAirlinesWeb.Helpers
{
    /// <summary>
    /// Provides helper methods for sending emails using SMTP settings configured in the application.
    /// </summary>
    public class MailHelper : IMailHelper
    {
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the MailHelper class.
        /// </summary>
        /// <param name="configuration">The application's configuration, used to retrieve SMTP settings.</param>
        public MailHelper(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        /// <summary>
        /// Asynchronously sends an email to a specified recipient with a given subject and body.
        /// SMTP server details, credentials, and sender information are retrieved from configuration.
        /// </summary>
        /// <param name="to">The recipient's email address.</param>
        /// <param name="subject">The subject line of the email.</param>
        /// <param name="body">The HTML content of the email body.</param>
        /// <returns>
        /// Task: An ApiResponse indicating the success or failure of the email sending operation,
        /// with an error message if it fails.
        /// </returns>
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
                    await client.ConnectAsync(smtp, int.Parse(port), false); // Use 'false' for UseSsl if port is typically 587 with STARTTLS, or 'true' for 465.
                    await client.AuthenticateAsync(from, password);
                    await client.SendAsync(message);
                    await client.DisconnectAsync(true); // Disconnect and dispose client
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse
                {
                    IsSuccess = false,
                    Message = ex.ToString() // Capture the full exception details
                };
            }

            return new ApiResponse
            {
                IsSuccess = true,
            };
        }



        /// <summary>
        /// Asynchronously sends a booking confirmation email to a customer with details about their flight ticket.
        /// </summary>
        /// <param name="email">The customer's email address.</param>
        /// <param name="fullName">The full name of the customer.</param>
        /// <param name="flightNumber">The flight number of the booked flight.</param>
        /// <param name="seatDisplay">A formatted string representing the booked seat (e.g., "12A Economy").</param>
        /// <param name="price">The price paid for the ticket.</param>
        /// <param name="purchaseDateUtc">The UTC date and time when the ticket was purchased.</param>
        /// <returns>
        /// Task: An ApiResponse indicating the success or failure of sending the confirmation email.
        /// </returns>
        public async Task<ApiResponse> SendBookingConfirmationEmailAsync(
        string email,
        string fullName,
        string flightNumber,
        string seatDisplay,
        decimal price,
        DateTime purchaseDateUtc)
        {
            var localPurchaseDate = TimezoneHelper.ConvertToLocal(purchaseDateUtc); // Convert UTC to local time for display

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

            // Reuse the generic SendEmailAsync method to send the formatted confirmation email.
            return await SendEmailAsync(email, "Flight Ticket Confirmation - Vitoria Airlines", body);
        }
    }
}