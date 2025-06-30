using NuGet.Protocol.Plugins;

namespace VitoriaAirlinesWeb.Services
{
    public interface IPaymentService
    {
        Task<string> CreateCheckoutSessionAsync(
            string flightNumber,
            string seatLabel,
            string originIATA,
            string originName,
            string destinationIATA,
            string destinationName,
            string seatClass,
            DateTime departureTime,
            decimal price,
            string successUrl,
            string cancelUrl);

        Task<string> CreateSeatUpgradeCheckoutSessionAsync(
            string description, 
            decimal priceDifference,
            string successUrl,
            string cancelUrl);
    }
}
