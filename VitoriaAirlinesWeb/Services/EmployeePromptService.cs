using System.Text;
using VitoriaAirlinesWeb.Data.Repositories;
using VitoriaAirlinesWeb.Responses;

namespace VitoriaAirlinesWeb.Services
{
    /// <summary>
    /// Provides services for processing chat prompts specifically for users with the Employee role.
    /// It interprets commands related to flights, airplanes, and airports, generating appropriate responses with data or links.
    /// </summary>
    public class EmployeePromptService : IEmployeePromptService
    {
        private readonly IFlightRepository _flightRepository;
        private readonly IAirplaneRepository _airplaneRepository;
        private readonly IAirportRepository _airportRepository;
        private readonly string _baseUrl;

        /// <summary>
        /// Initializes a new instance of the EmployeePromptService with necessary repositories and configuration.
        /// </summary>
        /// <param name="flightRepository">Repository for flight data access.</param>
        /// <param name="airplaneRepository">Repository for airplane data access.</param>
        /// <param name="airportRepository">Repository for airport data access.</param>
        /// <param name="configuration">Application configuration for retrieving base URL.</param>
        public EmployeePromptService(
            IFlightRepository flightRepository,
            IAirplaneRepository airplaneRepository,
            IAirportRepository airportRepository,
            IConfiguration configuration)
        {
            _flightRepository = flightRepository;
            _airplaneRepository = airplaneRepository;
            _airportRepository = airportRepository;
            _baseUrl = configuration["App:BaseUrl"]!; // Retrieves the base URL from app settings.
        }

        /// <summary>
        /// Processes a given prompt string from an Employee user and generates a relevant API response.
        /// It checks for keywords to determine the intended action (e.g., view today's flights, schedule flight, list airplanes/airports).
        /// </summary>
        /// <param name="prompt">The prompt string provided by the user (expected to be lowercase).</param>
        /// <returns>
        /// Task: An ApiResponse containing the message and results of the processed prompt,
        /// or null if the prompt does not match any predefined Employee actions.
        /// </returns>
        public async Task<ApiResponse?> ProcessPromptAsync(string prompt)
        {
            prompt = prompt.ToLower();

            // Handles prompts related to viewing today's scheduled flights.
            if (prompt.Contains("today") && prompt.Contains("scheduled") && prompt.Contains("flights"))
            {
                var today = DateTime.Today;
                var todayFlights = await _flightRepository.GetFlightsForDateAsync(today); // Retrieves flights scheduled for today.

                var resultText = new StringBuilder();
                resultText.AppendLine($"Scheduled flights for today ({today:dd/MM/yyyy}):");

                // Appends details for each flight.
                foreach (var flight in todayFlights)
                {
                    resultText.AppendLine($"• {flight.FlightNumber}: {flight.OriginAirport.IATA} → {flight.DestinationAirport.IATA} at {flight.DepartureUtc:HH:mm}");
                }

                return new ApiResponse
                {
                    IsSuccess = true,
                    Message = "Today's flights:",
                    Results = resultText.ToString() // Returns the list of today's flights.
                };
            }

            // Handles prompts related to scheduling a new flight.
            if (prompt.Contains("schedule") && prompt.Contains("flight"))
            {
                return new ApiResponse
                {
                    IsSuccess = true,
                    Message = "Click below to schedule a new flight.",
                    Results = $"To schedule a flight, go to {_baseUrl}/Flights/Create." // Provides a direct link to the flight creation page.
                };
            }

            // Handles general prompts about viewing or listing all scheduled flights.
            if (prompt.Contains("flights") && (prompt.Contains("view") || prompt.Contains("list")))
            {
                var flights = await _flightRepository.GetScheduledFlightsAsync(); // Retrieves all future scheduled flights.

                var resultText = new StringBuilder();
                resultText.AppendLine("Scheduled flights:");

                // Appends details for each scheduled flight.
                foreach (var flight in flights)
                {
                    resultText.AppendLine($"• {flight.FlightNumber}: {flight.OriginAirport.IATA} → {flight.DestinationAirport.IATA} on {flight.DepartureUtc:dd/MM/yyyy HH:mm}");
                }

                return new ApiResponse
                {
                    IsSuccess = true,
                    Message = "List of scheduled flights:",
                    Results = resultText.ToString() // Returns the list of scheduled flights.
                };
            }

            // Handles prompts related to viewing or listing active airplanes.
            if (prompt.Contains("airplanes") && (prompt.Contains("view") || prompt.Contains("list")))
            {
                var airplanes = _airplaneRepository.GetActiveAirplanes(); // Retrieves all active airplanes.

                var resultText = new StringBuilder();
                resultText.AppendLine("Active airplanes:");

                // Appends each active airplane model to the result.
                foreach (var plane in airplanes)
                {
                    resultText.AppendLine($"• {plane.Model}");
                }

                return new ApiResponse
                {
                    IsSuccess = true,
                    Message = "List of active airplane models:",
                    Results = resultText.ToString() // Returns the list of active airplanes.
                };
            }

            // Handles prompts related to listing or viewing airports/cities.
            if (prompt.Contains("airports") || prompt.Contains("cities"))
            {
                var airports = _airportRepository.GetAll(); // Retrieves all airport entities.

                var resultText = new StringBuilder();
                resultText.AppendLine("Available airports:");

                // Appends full name and IATA for each airport.
                foreach (var airport in airports)
                {
                    resultText.AppendLine($"• {airport.FullName} ({airport.IATA})");
                }

                return new ApiResponse
                {
                    IsSuccess = true,
                    Message = "List of airports:",
                    Results = resultText.ToString() // Returns the list of airports.
                };
            }

            // Returns null if no specific Employee action is matched by the prompt.
            return null;
        }
    }
}