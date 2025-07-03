using System.Text;
using VitoriaAirlinesWeb.Data.Repositories;
using VitoriaAirlinesWeb.Responses;

namespace VitoriaAirlinesWeb.Services
{
    public class AnonymousPromptService : IAnonymousPromptService
    {
        private readonly string _baseUrl;
        private readonly IAirportRepository _airportRepository;

        public AnonymousPromptService(
            IConfiguration configuration,
            IAirportRepository airportRepository)
        {
            _baseUrl = configuration["App:BaseUrl"]!;
            _airportRepository = airportRepository;
        }


        public async Task<ApiResponse?> ProcessPromptAsync(string prompt)
        {
            if (prompt.Contains("buy") && prompt.Contains("ticket"))
            {
                return new ApiResponse
                {
                    IsSuccess = true,
                    Message = "Click below to start your ticket purchase.",
                    Results = $"Go to {_baseUrl}/Home/Index to buy a ticket."
                };
            }

            if (prompt.Contains("destinations") || prompt.Contains("available cities"))
            {
                var cities = _airportRepository.GetAll();
                var resultText = new StringBuilder();
                resultText.AppendLine("Available destinations:");

                foreach (var city in cities)
                {
                    resultText.AppendLine($"• {city.FullName} ({city.IATA})");
                }

                return new ApiResponse
                {
                    IsSuccess = true,
                    Message = "Here is the list of available destinations:",
                    Results = resultText.ToString()
                };
            }

            if (prompt.Contains("create") && prompt.Contains("account"))
            {
                return new ApiResponse
                {
                    IsSuccess = true,
                    Message = "Click below to create an account.",
                    Results = $"Go to {_baseUrl}/Account/Register to create your account."
                };
            }

            return null;
        }
    }
}
