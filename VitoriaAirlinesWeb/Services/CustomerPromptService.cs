using System.Text;
using VitoriaAirlinesWeb.Data.Repositories;
using VitoriaAirlinesWeb.Responses;

namespace VitoriaAirlinesWeb.Services
{
    public class CustomerPromptService : ICustomerPromptService
    {
        private readonly IFlightRepository _flightRepository;
        private readonly string _baseUrl;

        public CustomerPromptService(
            IFlightRepository flightRepository,
            IConfiguration configuration)
        {
            _flightRepository = flightRepository;
            _baseUrl = configuration["App:BaseUrl"]!;
        }

        public async Task<ApiResponse?> ProcessPromptAsync(string prompt)
        {
            prompt = prompt.ToLower();

            
            if ((prompt.Contains("history") || prompt.Contains("past")) && prompt.Contains("my"))
            {
                return new ApiResponse
                {
                    IsSuccess = true,
                    Message = "Click below to view your flight history.",
                    Results = $"{_baseUrl}/MyFlights/History"
                };
            }

           
            if ((prompt.Contains("upcoming") || prompt.Contains("scheduled") || prompt.Contains("future")) && prompt.Contains("my"))
            {
                return new ApiResponse
                {
                    IsSuccess = true,
                    Message = "Click below to view your scheduled flights.",
                    Results = $"{_baseUrl}/MyFlights/Upcoming"
                };
            }


            if (prompt.Contains("flights") && (prompt.Contains("view") || prompt.Contains("search") || prompt.Contains("available")))
            {
                var futureFlights = await _flightRepository.GetScheduledFlightsAsync();

                var resultText = new StringBuilder();
                resultText.AppendLine("Available future flights:");

                foreach (var flight in futureFlights)
                {
                    resultText.AppendLine($"• {flight.FlightNumber}: {flight.OriginAirport.IATA} → {flight.DestinationAirport.IATA} on {flight.DepartureUtc:dd/MM/yyyy HH:mm}");
                }

                return new ApiResponse
                {
                    IsSuccess = true,
                    Message = "Here are the upcoming flights:",
                    Results = resultText.ToString()
                };
            }


            if (prompt.Contains("profile") || prompt.Contains("edit profile") || prompt.Contains("update profile"))
            {
                return new ApiResponse
                {
                    IsSuccess = true,
                    Message = "Access your profile to update your personal data.",
                    Results = $"{_baseUrl}/Customer/EditTravellerProfile"
                };
            }

            return null;
        }

    }
}
