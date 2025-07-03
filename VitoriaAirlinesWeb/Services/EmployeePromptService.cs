using System.Text;
using VitoriaAirlinesWeb.Data.Repositories;
using VitoriaAirlinesWeb.Responses;

namespace VitoriaAirlinesWeb.Services
{
    public class EmployeePromptService : IEmployeePromptService
    {
        private readonly IFlightRepository _flightRepository;
        private readonly IAirplaneRepository _airplaneRepository;
        private readonly IAirportRepository _airportRepository;
        private readonly string _baseUrl;

        public EmployeePromptService(
            IFlightRepository flightRepository,
            IAirplaneRepository airplaneRepository,
            IAirportRepository airportRepository,
            IConfiguration configuration)
        {
            _flightRepository = flightRepository;
            _airplaneRepository = airplaneRepository;
            _airportRepository = airportRepository;
            _baseUrl = configuration["App:BaseUrl"]!;
        }


      
        public async Task<ApiResponse?> ProcessPromptAsync(string prompt)
        {
            prompt = prompt.ToLower();

            if (prompt.Contains("today") && prompt.Contains("scheduled") && prompt.Contains("flights"))
            {
                var today = DateTime.Today;
                var todayFlights = await _flightRepository.GetFlightsForDateAsync(today);

                var resultText = new StringBuilder();
                resultText.AppendLine($"Scheduled flights for today ({today:dd/MM/yyyy}):");

                foreach (var flight in todayFlights)
                {
                    resultText.AppendLine($"• {flight.FlightNumber}: {flight.OriginAirport.IATA} → {flight.DestinationAirport.IATA} at {flight.DepartureUtc:HH:mm}");
                }

                return new ApiResponse
                {
                    IsSuccess = true,
                    Message = "Today's flights:",
                    Results = resultText.ToString()
                };
            }

            if (prompt.Contains("schedule") && prompt.Contains("flight"))
            {
                return new ApiResponse
                {
                    IsSuccess = true,
                    Message = "Click below to schedule a new flight.",
                    Results = $"To schedule a flight, go to {_baseUrl}/Flights/Create."
                };
            }

            if (prompt.Contains("flights") && (prompt.Contains("view") || prompt.Contains("list")))
            {
                var flights = await _flightRepository.GetScheduledFlightsAsync();

                var resultText = new StringBuilder();
                resultText.AppendLine("Scheduled flights:");

                foreach (var flight in flights)
                {
                    resultText.AppendLine($"• {flight.FlightNumber}: {flight.OriginAirport.IATA} → {flight.DestinationAirport.IATA} on {flight.DepartureUtc:dd/MM/yyyy HH:mm}");
                }

                return new ApiResponse
                {
                    IsSuccess = true,
                    Message = "List of scheduled flights:",
                    Results = resultText.ToString()
                };
            }

            // Ver aviões ativos
            if (prompt.Contains("airplanes") && (prompt.Contains("view") || prompt.Contains("list")))
            {
                var airplanes = _airplaneRepository.GetActiveAirplanes();

                var resultText = new StringBuilder();
                resultText.AppendLine("Active airplanes:");

                foreach (var plane in airplanes)
                {
                    resultText.AppendLine($"• {plane.Model}");
                }

                return new ApiResponse
                {
                    IsSuccess = true,
                    Message = "List of active airplane models:",
                    Results = resultText.ToString()
                };
            }

            if (prompt.Contains("airports") || prompt.Contains("cities"))
            {
                var airports = _airportRepository.GetAll();

                var resultText = new StringBuilder();
                resultText.AppendLine("Available airports:");

                foreach (var airport in airports)
                {
                    resultText.AppendLine($"• {airport.FullName} ({airport.IATA})");
                }

                return new ApiResponse
                {
                    IsSuccess = true,
                    Message = "List of airports:",
                    Results = resultText.ToString()
                };
            }

            return null;
        }

    }
}
