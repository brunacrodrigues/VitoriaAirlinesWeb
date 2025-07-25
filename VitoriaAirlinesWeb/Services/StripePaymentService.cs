using Microsoft.Extensions.Options;
using Stripe.Checkout;
using VitoriaAirlinesWeb.Configuration;
using VitoriaAirlinesWeb.Data.Entities;

namespace VitoriaAirlinesWeb.Services
{
    public class StripePaymentService : IPaymentService
    {
        private readonly StripeSettings _stripeSettings;

        public StripePaymentService(IOptions<StripeSettings> options)
        {
            _stripeSettings = options.Value;
        }

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
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
            {
                new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = "eur",
                        UnitAmount = (long)(price * 100),
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = $"Flight {flightNumber} – Seat {seatLabel} ({seatClass})",
                            Description = $"From {originIATA} ({originName}) to {destinationIATA} ({destinationName})\n" +
                            $"{departureTime:dd MMM yyyy 'at' HH:mm}"
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
                        UnitAmount = (long)(priceDifference * 100),
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
                LineItems = new List<SessionLineItemOptions>

                {
                    new SessionLineItemOptions
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
                    new SessionLineItemOptions
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

    }
}
