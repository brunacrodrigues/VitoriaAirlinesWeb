using System.Text;
using VitoriaAirlinesWeb.Data.Repositories;
using VitoriaAirlinesWeb.Responses;

namespace VitoriaAirlinesWeb.Services
{
    /// <summary>
    /// Provides services for processing chat prompts specifically for anonymous (non-authenticated) users.
    /// It interprets commands related to buying tickets, viewing destinations, and creating accounts,
    /// generating appropriate responses with links to relevant application pages.
    /// </summary>
    public class AnonymousPromptService : IAnonymousPromptService
    {
        private readonly string _baseUrl;
        private readonly IAirportRepository _airportRepository;


        /// <summary>
        /// Initializes a new instance of the AnonymousPromptService with necessary configuration and repositories.
        /// </summary>
        /// <param name="configuration">Application configuration for retrieving base URL.</param>
        /// <param name="airportRepository">Repository for airport data access.</param>
        public AnonymousPromptService(
            IConfiguration configuration,
            IAirportRepository airportRepository)
        {
            _baseUrl = configuration["App:BaseUrl"]!; // Retrieves the base URL from app settings.
            _airportRepository = airportRepository;
        }



        /// <summary>
        /// Processes a given prompt string from an anonymous user and generates a relevant API response.
        /// It checks for keywords to determine the intended action (e.g., buy ticket, list destinations, create account).
        /// </summary>
        /// <param name="prompt">The prompt string provided by the user (expected to be lowercase).</param>
        /// <returns>
        /// Task: An ApiResponse containing the message and results of the processed prompt,
        /// or null if the prompt does not match any predefined anonymous actions.
        /// </returns>
        public async Task<ApiResponse?> ProcessPromptAsync(string prompt)
        {

            // Handles prompts related to buying tickets.
            if (prompt.Contains("buy") && prompt.Contains("ticket"))
            {
                return new ApiResponse
                {
                    IsSuccess = true,
                    Message = "Click below to start your ticket purchase.",
                    Results = $"Go to {_baseUrl}/Home/Index to buy a ticket." // Provides a direct link to the home page for ticket purchase.
                };
            }

            // Handles prompts related to viewing available destinations or cities.
            if (prompt.Contains("destinations") || prompt.Contains("available cities"))
            {
                var cities = _airportRepository.GetAll(); // Retrieves all airport entities.
                var resultText = new StringBuilder();
                resultText.AppendLine("Available destinations:");

                // Appends full name and IATA for each airport to the result.
                foreach (var city in cities)
                {
                    resultText.AppendLine($"• {city.FullName} ({city.IATA})");
                }

                return new ApiResponse
                {
                    IsSuccess = true,
                    Message = "Here is the list of available destinations:",
                    Results = resultText.ToString() // Returns the list of available airports.
                };
            }

            // Handles prompts related to creating a new account.
            if (prompt.Contains("create") && prompt.Contains("account"))
            {
                return new ApiResponse
                {
                    IsSuccess = true,
                    Message = "Click below to create an account.",
                    Results = $"Go to {_baseUrl}/Account/Register to create your account." // Provides a direct link to the registration page.
                };
            }

            // Returns null if no specific anonymous action is matched by the prompt.
            return null;
        }
    }
}