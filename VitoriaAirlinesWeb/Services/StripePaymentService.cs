using Microsoft.Extensions.Options;
using Stripe.Checkout;
using VitoriaAirlinesWeb.Configuration;
using VitoriaAirlinesWeb.Data;
using VitoriaAirlinesWeb.Data.Entities;
using VitoriaAirlinesWeb.Data.Enums;
using VitoriaAirlinesWeb.Data.Repositories;

namespace VitoriaAirlinesWeb.Services
{
    /// <summary>
    /// Provides payment processing services using Stripe Checkout.
    /// It creates Stripe Checkout Sessions for single tickets, seat upgrades, and round-trip bookings.
    /// </summary>
    public class StripePaymentService : IPaymentService
    {
        private readonly StripeSettings _stripeSettings;
        private readonly IFlightRepository _flightRepository;

        /// <summary>
        /// Initializes a new instance of the StripePaymentService class.
        /// </summary>
        /// <param name="options">Options containing Stripe API settings.</param>
        public StripePaymentService(IOptions<StripeSettings> options, IFlightRepository flightRepository)
        {
            _stripeSettings = options.Value;
            _flightRepository = flightRepository;
        }


        /// <summary>
        /// Asynchronously creates a Stripe Checkout Session for a single flight ticket purchase.
        /// </summary>
        /// <param name="flightNumber">The flight number.</param>
        /// <param name="seatLabel">The label of the selected seat (e.g., "12A").</param>
        /// <param name="originIATA">The IATA code of the origin airport.</param>
        /// <param name="originName">The full name of the origin airport.</param>
        /// <param name="destinationIATA">The IATA code of the destination airport.</param>
        /// <param name="destinationName">The full name of the destination airport.</param>
        /// <param name="seatClass">The class of the seat (e.g., "Economy", "Executive").</param>
        /// <param name="departureTime">The local departure time of the flight.</param>
        /// <param name="price">The price of the ticket in decimal (e.g., 100.50).</param>
        /// <param name="successUrl">The URL to redirect to upon successful payment.</param>
        /// <param name="cancelUrl">The URL to redirect to if payment is canceled.</param>
        /// <returns>
        /// Task: A string representing the URL for the Stripe Checkout Session. The customer will be redirected to this URL.
        /// </returns>
        public async Task<string> CreateCheckoutSessionAsync(
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
            string cancelUrl)
        {
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" }, // Only accept card payments.
                LineItems = new List<SessionLineItemOptions>
            {
                new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions // Defines the product and its pricing.
                    {
                        Currency = "eur", // Currency is Euro.
                        UnitAmount = (long)(price * 100), // Price must be in cents.
                        ProductData = new SessionLineItemPriceDataProductDataOptions // Product details displayed to customer.
                        {
                            Name = $"Flight {flightNumber} – Seat {seatLabel} ({seatClass})",
                            Description = $"From {originIATA} ({originName}) to {destinationIATA} ({destinationName})\n" +
                            $"{departureTime:dd MMM yyyy 'at' HH:mm}"
                        }
                    },
                    Quantity = 1 // Only one unit of this product.
                }
            },
                Mode = "payment", // Session is for a one-time payment.
                SuccessUrl = successUrl, // URL for successful payment redirect.
                CancelUrl = cancelUrl // URL for canceled payment redirect.
            };

            var service = new SessionService(); // Stripe service for creating sessions.
            var session = await service.CreateAsync(options); // Create the session.
            return session.Url; // Return the URL to redirect the customer to.
        }



        /// <summary>
        /// Asynchronously creates a Stripe Checkout Session for a seat upgrade.
        /// </summary>
        /// <param name="description">A textual description of the seat upgrade (e.g., "Upgrade to Executive Class").</param>
        /// <param name="priceDifference">The monetary difference (can be positive or negative, but typically positive for upgrade) for the upgrade in decimal.</param>
        /// <param name="successUrl">The URL to redirect to upon successful payment.</param>
        /// <param name="cancelUrl">The URL to redirect to if payment is canceled.</param>
        /// <returns>
        /// Task: A string representing the URL for the Stripe Checkout Session.
        /// </returns>
        public async Task<string> CreateSeatUpgradeCheckoutSessionAsync(
            string description,
            decimal priceDifference,
            string successUrl,
            string cancelUrl)
        {
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
            {
                new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = "eur",
                        UnitAmount = (long)(priceDifference * 100), // Price must be in cents.
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = "Seat Upgrade",
                            Description = description,
                        }
                    },
                    Quantity = 1
                }
            },
                Mode = "payment",
                SuccessUrl = successUrl,
                CancelUrl = cancelUrl
            };

            var service = new SessionService();
            var session = await service.CreateAsync(options);
            return session.Url;
        }



        /// <summary>
        /// Asynchronously creates a Stripe Checkout Session for a round-trip flight booking.
        /// This session includes two line items, one for the outbound flight and one for the return flight.
        /// </summary>
        /// <param name="outboundFlight">The Flight entity for the outbound journey.</param>
        /// <param name="returnFlight">The Flight entity for the return journey.</param>
        /// <param name="outboundSeat">The selected Seat for the outbound flight.</param>
        /// <param name="returnSeat">The selected Seat for the return flight.</param>
        /// <param name="outboundPrice">The calculated price for the outbound ticket.</param>
        /// <param name="returnPrice">The calculated price for the return ticket.</param>
        /// <param name="successUrl">The URL to redirect to upon successful payment.</param>
        /// <param name="cancelUrl">The URL to redirect to if payment is canceled.</param>
        /// <returns>
        /// Task: A string representing the URL for the Stripe Checkout Session.
        /// </returns>
        public async Task<string> CreateRoundTripCheckoutSessionAsync(
            Flight outboundFlight,
            Flight returnFlight,
            Seat outboundSeat,
            Seat returnSeat,
            decimal outboundPrice,
            decimal returnPrice,
            string successUrl,
            string cancelUrl)
        {
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions> // List of products/services being paid for.
                {
                    new SessionLineItemOptions // Outbound flight details.
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency = "eur",
                            UnitAmount = (long)(outboundPrice * 100),
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = $"Flight {outboundFlight.FlightNumber} – Seat {outboundSeat.Row}{outboundSeat.Letter} ({outboundSeat.Class})",
                                Description = $"From {outboundFlight.OriginAirport.IATA} ({outboundFlight.OriginAirport.Name}) to {outboundFlight.DestinationAirport.IATA} ({outboundFlight.DestinationAirport.Name})\n" +
                                              $"{outboundFlight.DepartureUtc:dd MMM yyyy 'at' HH:mm}"
                            }
                        },
                        Quantity = 1
                    },
                    new SessionLineItemOptions // Return flight details.
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency = "eur",
                            UnitAmount = (long)(returnPrice * 100),
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = $"Flight {returnFlight.FlightNumber} – Seat {returnSeat.Row}{returnSeat.Letter} ({returnSeat.Class})",
                                Description = $"From {returnFlight.OriginAirport.IATA} ({returnFlight.OriginAirport.Name}) to {returnFlight.DestinationAirport.IATA} ({returnFlight.DestinationAirport.Name})\n" +
                                              $"{returnFlight.DepartureUtc:dd MMM yyyy 'at' HH:mm}"
                            }
                        },
                        Quantity = 1
                    }
                },
                Mode = "payment",
                SuccessUrl = successUrl,
                CancelUrl = cancelUrl
            };

            var service = new SessionService();
            var session = await service.CreateAsync(options);
            return session.Url;
        }



        /// <summary>
        /// Asynchronously creates a Stripe Checkout Session for the MAUI API flow.
        /// Handles both one-way and round-trip bookings by dynamically building LineItems in a single transaction.
        /// </summary>
        public async Task<Session> CreateApiCheckoutSessionAsync(
            BookingRequestDto request,
            Dictionary<string, string> metadata,
            string successUrl,
            string cancelUrl)
        {
            var lineItems = new List<SessionLineItemOptions>();

            foreach (var leg in request.Legs)
            {
                var flight = await _flightRepository.GetByIdWithAirplaneAndSeatsAsync(leg.FlightId);
                var seat = flight.Airplane.Seats.First(s => s.Id == leg.SeatId);
                var price = seat.Class == SeatClass.Economy ? flight.EconomyClassPrice : flight.ExecutiveClassPrice;

                lineItems.Add(new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = "eur",
                        UnitAmount = (long)(price * 100),
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = $"Flight {flight.FlightNumber} - {seat.Class} Seat {seat.Row}{seat.Letter}",
                            Description = $"Travel: {flight.OriginAirport.IATA} to {flight.DestinationAirport.IATA} - Departure: {flight.DepartureUtc:dd MMM yyyy 'at' HH:mm}"
                        }
                    },
                    Quantity = 1
                });
            }

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = lineItems,
                Mode = "payment",
                SuccessUrl = successUrl,
                CancelUrl = cancelUrl,
                Metadata = metadata
            };

            var service = new SessionService();
            var session = await service.CreateAsync(options);
            return session;
        }
    }


}
