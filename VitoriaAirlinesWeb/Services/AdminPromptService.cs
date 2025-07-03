using System.Text;
using VitoriaAirlinesWeb.Data.Repositories;
using VitoriaAirlinesWeb.Responses;

namespace VitoriaAirlinesWeb.Services
{
    public class AdminPromptService : IAdminPromptService
    {
        private readonly IAirplaneRepository _airplaneRepository;
        private readonly IFlightRepository _flightRepository;
        private readonly IAirportRepository _airportRepository;
        private readonly string _baseUrl;

        public AdminPromptService(
            IAirplaneRepository airplaneRepository,
            IFlightRepository flightRepository,
            IAirportRepository airportRepository,
            IConfiguration configuration)
        {
            _airplaneRepository = airplaneRepository;
            _flightRepository = flightRepository;
            _airportRepository = airportRepository;
            _baseUrl = configuration["App:BaseUrl"]!;
        }


        public async Task<ApiResponse?> ProcessPromptAsync(string prompt)
        {
            if (prompt.Contains("airplanes") && (prompt.Contains("view") || prompt.Contains("list")))
            {
                var airplanes = _airplaneRepository.GetAll();

                var resultText = new StringBuilder();
                foreach (var plane in airplanes)
                {
                    resultText.AppendLine($"• {plane.Model}");
                }

                return new ApiResponse
                {
                    IsSuccess = true,
                    Message = "Here are the registered airplanes:",
                    Results = resultText.ToString()
                };
            }

            if (prompt.Contains("flights") && prompt.Contains("today"))
            {
                var today = DateTime.Today;
                var flights = await _flightRepository.GetFlightsForDateAsync(today);
                var airportNames = flights
                    .SelectMany(f => new[] { f.OriginAirport.FullName, f.DestinationAirport.FullName })
                    .Distinct()
                    .ToList();

                var resultText = new StringBuilder();
                resultText.AppendLine($"Flights scheduled for today ({today:dd/MM/yyyy}):");

                foreach (var flight in flights)
                {
                    resultText.AppendLine($"• {flight.FlightNumber}: {flight.OriginAirport.IATA} → {flight.DestinationAirport.IATA} ({flight.DepartureUtc:HH:mm})");
                }

                resultText.AppendLine();
                resultText.AppendLine("Airports involved:");
                foreach (var airport in airportNames)
                {
                    resultText.AppendLine($"- {airport}");
                }

                return new ApiResponse
                {
                    IsSuccess = true,
                    Message = "Flights and airports:",
                    Results = resultText.ToString()
                };
            }

            if (prompt.Contains("cities") || (prompt.Contains("airports") && (prompt.Contains("view") || prompt.Contains("list"))))
            {
                var cities = _airportRepository.GetAll();

                var resultText = new StringBuilder();
                resultText.AppendLine("Available airports:");

                foreach (var city in cities)
                {
                    resultText.AppendLine($"• {city.FullName} ({city.IATA})");
                }

                return new ApiResponse
                {
                    IsSuccess = true,
                    Message = "Here is the list of airports:",
                    Results = resultText.ToString()
                };
            }

            if (prompt.Contains("create") && prompt.Contains("airplane"))
            {
                return new ApiResponse
                {
                    IsSuccess = true,
                    Message = "Click below to create a new airplane.",
                    Results = $"To register a new airplane go to {_baseUrl}/Airplanes/Create."
                };
            }

            return null;
        }
    }
}