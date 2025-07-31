using System.Text;
using VitoriaAirlinesWeb.Data.Repositories;
using VitoriaAirlinesWeb.Responses;

namespace VitoriaAirlinesWeb.Services
{
    /// <summary>
    /// Provides services for processing chat prompts specifically for authenticated Customer users.
    /// It interprets commands related to viewing flight history, upcoming flights, available flights,
    /// and managing their profile, generating appropriate responses with links or data.
    /// </summary>
    public class CustomerPromptService : ICustomerPromptService
    {
        private readonly IFlightRepository _flightRepository;
        private readonly string _baseUrl;

        /// <summary>
        /// Initializes a new instance of the CustomerPromptService with necessary repositories and configuration.
        /// </summary>
        /// <param name="flightRepository">Repository for flight data access.</param>
        /// <param name="configuration">Application configuration for retrieving base URL.</param>
        public CustomerPromptService(
            IFlightRepository flightRepository,
            IConfiguration configuration)
        {
            _flightRepository = flightRepository;
            _baseUrl = configuration["App:BaseUrl"]!; // Retrieves the base URL from app settings.
        }


        /// <summary>
        /// Processes a given prompt string from a Customer user and generates a relevant API response.
        /// It checks for keywords to determine the intended action (e.g., view flight history, upcoming flights, edit profile).
        /// </summary>
        /// <param name="prompt">The prompt string provided by the user (expected to be lowercase).</param>
        /// <returns>
        /// Task: An ApiResponse containing the message and results of the processed prompt,
        /// or null if the prompt does not match any predefined Customer actions.
        /// </returns>
        public async Task<ApiResponse?> ProcessPromptAsync(string prompt)
        {
            prompt = prompt.ToLower();

            // Handles prompts related to viewing the user's flight history or past flights.
            if ((prompt.Contains("history") || prompt.Contains("past")) && prompt.Contains("my"))
            {
                return new ApiResponse
                {
                    IsSuccess = true,
                    Message = "Click below to view your flight history.",
                    Results = $"{_baseUrl}/MyFlights/History" // Provides a direct link to the flight history page.
                };
            }

            // Handles prompts related to viewing the user's upcoming, scheduled, or future flights.
            if ((prompt.Contains("upcoming") || prompt.Contains("scheduled") || prompt.Contains("future")) && prompt.Contains("my"))
            {
                return new ApiResponse
                {
                    IsSuccess = true,
                    Message = "Click below to view your scheduled flights.",
                    Results = $"{_baseUrl}/MyFlights/Upcoming" // Provides a direct link to the upcoming flights page.
                };
            }

            // Handles general prompts about viewing, searching, or listing available flights.
            if (prompt.Contains("flights") && (prompt.Contains("view") || prompt.Contains("search") || prompt.Contains("available")))
            {
                var futureFlights = await _flightRepository.GetScheduledFlightsAsync(); // Retrieves all future scheduled flights.

                var resultText = new StringBuilder();
                resultText.AppendLine("Available future flights:");

                // Appends details for each available flight.
                foreach (var flight in futureFlights)
                {
                    resultText.AppendLine($"• {flight.FlightNumber}: {flight.OriginAirport.IATA} → {flight.DestinationAirport.IATA} on {flight.DepartureUtc:dd/MM/yyyy HH:mm}");
                }

                return new ApiResponse
                {
                    IsSuccess = true,
                    Message = "Here are the upcoming flights:",
                    Results = resultText.ToString() // Returns the list of available flights.
                };
            }

            // Handles prompts related to editing or updating the user's profile.
            if (prompt.Contains("profile") || prompt.Contains("edit profile") || prompt.Contains("update profile"))
            {
                return new ApiResponse
                {
                    IsSuccess = true,
                    Message = "Access your profile to update your personal data.",
                    Results = $"{_baseUrl}/Customer/EditTravellerProfile" // Provides a direct link to the customer profile edit page.
                };
            }

            // Returns null if no specific Customer action is matched by the prompt.
            return null;
        }
    }
}