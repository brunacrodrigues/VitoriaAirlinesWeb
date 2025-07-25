using VitoriaAirlinesWeb.Data.Entities;

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

        Task<string> CreateRoundTripCheckoutSessionAsync(
            Flight outboundFlight,
            Flight returnFlight,
            Seat outboundSeat,
            Seat returnSeat,
            decimal outboundPrice,
            decimal returnPrice,
            string successUrl,
            string cancelUrl);
    }
}
