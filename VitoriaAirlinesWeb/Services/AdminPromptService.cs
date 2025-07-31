using System.Text;
using VitoriaAirlinesWeb.Data.Repositories;
using VitoriaAirlinesWeb.Responses;

namespace VitoriaAirlinesWeb.Services
{
    /// <summary>
    /// Provides services for processing chat prompts specifically for users with the Admin role.
    /// It interprets commands related to airplanes, flights, and airports, and generates appropriate responses.
    /// </summary>
    public class AdminPromptService : IAdminPromptService
    {
        private readonly IAirplaneRepository _airplaneRepository;
        private readonly IFlightRepository _flightRepository;
        private readonly IAirportRepository _airportRepository;
        private readonly string _baseUrl;



        /// <summary>
        /// Initializes a new instance of the AdminPromptService with necessary repositories and configuration.
        /// </summary>
        /// <param name="airplaneRepository">Repository for airplane data access.</param>
        /// <param name="flightRepository">Repository for flight data access.</param>
        /// <param name="airportRepository">Repository for airport data access.</param>
        /// <param name="configuration">Application configuration for retrieving base URL.</param>
        public AdminPromptService(
            IAirplaneRepository airplaneRepository,
            IFlightRepository flightRepository,
            IAirportRepository airportRepository,
            IConfiguration configuration)
        {
            _airplaneRepository = airplaneRepository;
            _flightRepository = flightRepository;
            _airportRepository = airportRepository;
            _baseUrl = configuration["App:BaseUrl"]!; // Retrieves the base URL from app settings.
        }


        /// <summary>
        /// Processes a given prompt string from an Admin user and generates a relevant API response.
        /// It checks for keywords to determine the intended action (e.g., list airplanes, view flights, create airplane).
        /// </summary>
        /// <param name="prompt">The prompt string provided by the user (expected to be lowercase).</param>
        /// <returns>
        /// Task: An ApiResponse containing the message and results of the processed prompt,
        /// or null if the prompt does not match any predefined Admin actions.
        /// </returns>
        public async Task<ApiResponse?> ProcessPromptAsync(string prompt)
        {

            // Handles prompts related to listing or viewing airplanes.
            if (prompt.Contains("airplanes") && (prompt.Contains("view") || prompt.Contains("list")))
            {
                var airplanes = _airplaneRepository.GetAll(); // Retrieves all airplane entities.

                var resultText = new StringBuilder();
                foreach (var plane in airplanes)
                {
                    resultText.AppendLine($"• {plane.Model}"); // Appends each airplane model to the result.
                }

                return new ApiResponse
                {
                    IsSuccess = true,
                    Message = "Here are the registered airplanes:",
                    Results = resultText.ToString() // Returns the list of airplanes.
                };
            }

            // Handles prompts related to viewing today's scheduled flights.
            if (prompt.Contains("flights") && prompt.Contains("today"))
            {
                var today = DateTime.Today;
                var flights = await _flightRepository.GetFlightsForDateAsync(today); // Retrieves flights scheduled for today.

                // Extracts distinct airport names involved in today's flights.
                var airportNames = flights
                    .SelectMany(f => new[] { f.OriginAirport.FullName, f.DestinationAirport.FullName })
                    .Distinct()
                    .ToList();

                var resultText = new StringBuilder();
                resultText.AppendLine($"Flights scheduled for today ({today:dd/MM/yyyy}):");

                // Appends details for each flight.
                foreach (var flight in flights)
                {
                    resultText.AppendLine($"• {flight.FlightNumber}: {flight.OriginAirport.IATA} → {flight.DestinationAirport.IATA} ({flight.DepartureUtc:HH:mm})");
                }

                resultText.AppendLine();
                resultText.AppendLine("Airports involved:");
                // Appends each involved airport name.
                foreach (var airport in airportNames)
                {
                    resultText.AppendLine($"- {airport}");
                }

                return new ApiResponse
                {
                    IsSuccess = true,
                    Message = "Flights and airports:",
                    Results = resultText.ToString() // Returns the flight and airport information.
                };
            }

            // Handles prompts related to listing or viewing airports/cities.
            if (prompt.Contains("cities") || (prompt.Contains("airports") && (prompt.Contains("view") || prompt.Contains("list"))))
            {
                var cities = _airportRepository.GetAll(); // Retrieves all airport entities.

                var resultText = new StringBuilder();
                resultText.AppendLine("Available airports:");

                // Appends full name and IATA for each airport.
                foreach (var city in cities)
                {
                    resultText.AppendLine($"• {city.FullName} ({city.IATA})");
                }

                return new ApiResponse
                {
                    IsSuccess = true,
                    Message = "Here is the list of airports:",
                    Results = resultText.ToString() // Returns the list of airports.
                };
            }

            // Handles prompts related to creating a new airplane.
            if (prompt.Contains("create") && prompt.Contains("airplane"))
            {
                return new ApiResponse
                {
                    IsSuccess = true,
                    Message = "Click below to create a new airplane.",
                    Results = $"To register a new airplane go to {_baseUrl}/Airplanes/Create." // Provides a direct link.
                };
            }

            // Returns null if no specific Admin action is matched by the prompt.
            return null;
        }
    }
}