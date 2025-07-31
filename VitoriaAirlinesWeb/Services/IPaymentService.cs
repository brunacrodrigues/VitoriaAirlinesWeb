using VitoriaAirlinesWeb.Data.Entities;

namespace VitoriaAirlinesWeb.Services
{
    /// <summary>
    /// Defines the contract for a payment service, typically integrating with a payment gateway like Stripe.
    /// </summary>
    public interface IPaymentService
    {
        /// <summary>
        /// Asynchronously creates a checkout session for a single flight ticket.
        /// </summary>
        /// <param name="flightNumber">The flight number.</param>
        /// <param name="seatLabel">The seat label.</param>
        /// <param name="originIATA">The IATA code of the origin airport.</param>
        /// <param name="originName">The full name of the origin airport.</param>
        /// <param name="destinationIATA">The IATA code of the destination airport.</param>
        /// <param name="destinationName">The full name of the destination airport.</param>
        /// <param name="seatClass">The class of the seat (Economy/Executive).</param>
        /// <param name="departureTime">The departure time of the flight.</param>
        /// <param name="price">The price of the ticket.</param>
        /// <param name="successUrl">The URL to redirect to upon successful payment.</param>
        /// <param name="cancelUrl">The URL to redirect to if payment is canceled.</param>
        /// <returns>Task: The URL for the checkout session.</returns>
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



        /// <summary>
        /// Asynchronously creates a checkout session for a seat upgrade.
        /// </summary>
        /// <param name="description">A description for the upgrade.</param>
        /// <param name="priceDifference">The price difference for the upgrade.</param>
        /// <param name="successUrl">The URL to redirect to upon successful payment.</param>
        /// <param name="cancelUrl">The URL to redirect to if payment is canceled.</param>
        /// <returns>Task: The URL for the checkout session.</returns>
        Task<string> CreateSeatUpgradeCheckoutSessionAsync(
            string description,
            decimal priceDifference,
            string successUrl,
            string cancelUrl);



        /// <summary>
        /// Asynchronously creates a checkout session for a round-trip flight booking.
        /// </summary>
        /// <param name="outboundFlight">The outbound Flight entity.</param>
        /// <param name="returnFlight">The return Flight entity.</param>
        /// <param name="outboundSeat">The selected seat for the outbound flight.</param>
        /// <param name="returnSeat">The selected seat for the return flight.</param>
        /// <param name="outboundPrice">The price of the outbound ticket.</param>
        /// <param name="returnPrice">The price of the return ticket.</param>
        /// <param name="successUrl">The URL to redirect to upon successful payment.</param>
        /// <param name="cancelUrl">The URL to redirect to if payment is canceled.</param>
        /// <returns>Task: The URL for the checkout session.</returns>
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