using VitoriaAirlinesWeb.Responses;

namespace VitoriaAirlinesWeb.Helpers
{
    /// <summary>
    /// Defines the contract for a helper class that facilitates sending emails.
    /// </summary>
    public interface IMailHelper
    {
        /// <summary>
        /// Asynchronously sends an email to a specified recipient.
        /// </summary>
        /// <param name="to">The recipient's email address.</param>
        /// <param name="subject">The subject of the email.</param>
        /// <param name="body">The HTML body content of the email.</param>
        /// <returns>Task: An ApiResponse indicating the success or failure of the email sending operation.</returns>
        Task<ApiResponse> SendEmailAsync(string to, string subject, string body);


        /// <summary>
        /// Asynchronously sends a booking confirmation email to a customer.
        /// </summary>
        /// <param name="email">The customer's email address.</param>
        /// <param name="fullName">The full name of the customer.</param>
        /// <param name="flightNumber">The flight number of the booked flight.</param>
        /// <param name="seatDisplay">The display string for the booked seat (e.g., "12A Economy").</param>
        /// <param name="price">The price paid for the ticket.</param>
        /// <param name="purchaseDateUtc">The UTC date and time of the ticket purchase.</param>
        /// <returns>Task: An ApiResponse indicating the success or failure of sending the confirmation email.</returns>
        Task<ApiResponse> SendBookingConfirmationEmailAsync(
            string email,
            string fullName,
            string flightNumber,
            string seatDisplay,
            decimal price,
            DateTime purchaseDateUtc);

    }
}
