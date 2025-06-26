using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using System.Globalization;
using VitoriaAirlinesWeb.Data.Entities;
using VitoriaAirlinesWeb.Data.Enums;
using VitoriaAirlinesWeb.Data.Repositories;
using VitoriaAirlinesWeb.Helpers;
using VitoriaAirlinesWeb.Services;

namespace VitoriaAirlinesWeb.Controllers
{
    public class BookingController : Controller
    {
        private readonly IFlightRepository _flightRepository;
        private readonly ITicketRepository _ticketRepository;
        private readonly IConverterHelper _converterHelper;
        private readonly IPaymentService _paymentService;
        private readonly IUserHelper _userHelper;
        private readonly ICustomerProfileRepository _customerProfileRepository;
        private readonly IMailHelper _mailHelper;

        public BookingController(
            IFlightRepository flightRepository,
            ITicketRepository ticketRepository,
            IConverterHelper converterHelper,
            IPaymentService paymentService,
            IUserHelper userHelper,
            ICustomerProfileRepository customerProfileRepository,
            IMailHelper mailHelper)
        {
            _flightRepository = flightRepository;
            _ticketRepository = ticketRepository;
            _converterHelper = converterHelper;
            _paymentService = paymentService;
            _userHelper = userHelper;
            _customerProfileRepository = customerProfileRepository;
            _mailHelper = mailHelper;
        }

        [HttpGet]
        public async Task<IActionResult> SelectSeat(int flightId)
        {
            if (User.Identity.IsAuthenticated && (User.IsInRole(UserRoles.Admin) || User.IsInRole(UserRoles.Employee)))
            {
                TempData["Error"] = "This functionality is reserved for customers only.";
                return RedirectToAction("Index", "Home");
            }


            var flight = await _flightRepository.GetByIdWithAirplaneAndSeatsAsync(flightId);
            if (flight == null || flight.Status != FlightStatus.Scheduled)
            {
                TempData["Error"] = "This flight is no longer available for booking.";
                return RedirectToAction("Index", "Home");
            }

            var occupiedSeatIds = (await _ticketRepository.GetTicketsByFlightAsync(flightId))
                .Select(t => t.SeatId);

            var viewModel = _converterHelper.ToSelectSeatViewModelAsync(flight, occupiedSeatIds);

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmBooking(int flightId, int seatId)
        {
            if (User.Identity.IsAuthenticated && (User.IsInRole(UserRoles.Admin) || User.IsInRole(UserRoles.Employee)))
            {
                TempData["Error"] = "This functionality is reserved for customers only.";
                return RedirectToAction("Index", "Home");
            }

            var flight = await _flightRepository.GetByIdWithAirplaneAndSeatsAsync(flightId);
            if (flight == null)
            {
                return NotFound("Flight not found");
            }

            var seat = flight.Airplane.Seats.FirstOrDefault(s => s.Id == seatId);
            if (seat == null)
            {
                return NotFound("Seat not found");
            }

            var model = _converterHelper.ToConfirmBookingViewModel(flight, seat);
            model.IsCustomer = User.Identity.IsAuthenticated && User.IsInRole(UserRoles.Customer);

            return View(model);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCheckoutSession(int flightId, int seatId, decimal price, string? firstName, string? lastName, string? email)
        {
            var flight = await _flightRepository.GetByIdWithAirplaneAndSeatsAsync(flightId);
            if (flight == null) return NotFound("Flight not found.");

            var seat = flight.Airplane.Seats.FirstOrDefault(s => s.Id == seatId);
            if (seat == null) return NotFound("Seat not found.");


            HttpContext.Session.SetInt32("FlightId", flightId);
            HttpContext.Session.SetInt32("SeatId", seatId);
            HttpContext.Session.SetString("Price", price.ToString(CultureInfo.InvariantCulture));

            if (!User.Identity.IsAuthenticated || !User.IsInRole(UserRoles.Customer))
            {
                HttpContext.Session.SetString("FirstName", firstName ?? "");
                HttpContext.Session.SetString("LastName", lastName ?? "");
                HttpContext.Session.SetString("Email", email ?? "");
            }

            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var successUrl = $"{baseUrl}/Booking/Success?session_id={{CHECKOUT_SESSION_ID}}";
            var cancelUrl = $"{baseUrl}/Booking/Cancel";

            var checkoutUrl = await _paymentService.CreateCheckoutSessionAsync(
                flight.FlightNumber,
                $"{seat.Row}{seat.Letter}",
                flight.OriginAirport.IATA,
                flight.OriginAirport.Name,
                flight.DestinationAirport.IATA,
                flight.DestinationAirport.Name,
                seat.Class.ToString(),
                flight.DepartureUtc,
                price,
                successUrl,
                cancelUrl
            );

            return Redirect(checkoutUrl);
        }



        public async Task<IActionResult> Success(string session_id)
        {
            var sessionService = new SessionService();
            var session = await sessionService.GetAsync(session_id);

            if (session.PaymentStatus != "paid")
            {
                TempData["Error"] = "Payment was not completed.";
                return RedirectToAction("Index", "Home");
            }

            var flightId = HttpContext.Session.GetInt32("FlightId");
            var seatId = HttpContext.Session.GetInt32("SeatId");
            var priceString = HttpContext.Session.GetString("Price");

            if (flightId == null || seatId == null || string.IsNullOrEmpty(priceString))
            {
                TempData["Error"] = "Booking information is missing.";
                return RedirectToAction("Index", "Home");
            }

            decimal price = decimal.Parse(priceString, CultureInfo.InvariantCulture);
            string userId;

            if (User.Identity.IsAuthenticated && User.IsInRole(UserRoles.Customer))
            {
                var email = User.Identity.Name;
                var existingUser = await _userHelper.GetUserByEmailAsync(email);
                if (existingUser == null)
                {
                    TempData["Error"] = "Could not retrieve your user data.";
                    return RedirectToAction("Index", "Home");
                }

                userId = existingUser.Id;
            }
            else
            {
                var email = HttpContext.Session.GetString("Email");
                var firstName = HttpContext.Session.GetString("FirstName");
                var lastName = HttpContext.Session.GetString("LastName");

                if (string.IsNullOrWhiteSpace(email))
                {
                    TempData["Error"] = "Email information is missing.";
                    return RedirectToAction("Index", "Home");
                }

                var user = new User
                {
                    UserName = email,
                    Email = email,
                    FirstName = firstName,
                    LastName = lastName
                };

                var createResult = await _userHelper.AddUserAsync(user, "Temp1234!");
                if (!createResult.Succeeded)
                {
                    TempData["Error"] = "User creation failed.";
                    return RedirectToAction("Index", "Home");
                }

                var emailToken = await _userHelper.GenerateEmailConfirmationTokenAsync(user);
                await _userHelper.ConfirmEmailAsync(user, emailToken);
                await _userHelper.CheckRoleAsync(UserRoles.Customer);
                await _userHelper.AddUserToRoleAsync(user, UserRoles.Customer);

                var profile = new CustomerProfile
                {
                    UserId = user.Id
                };
                await _customerProfileRepository.CreateAsync(profile);

                var token = await _userHelper.GeneratePasswordResetTokenAsync(user);
                var scheme = Request?.Scheme ?? "https";
                var resetLink = Url.Action("ResetPassword", "Account", new
                {
                    email = user.Email,
                    token = token
                }, protocol: scheme);

                var body = $"<p>Hello {user.FullName},</p>" +
                           $"<p>Thank you for booking with Vitoria Airlines.</p>" +
                           $"<p>You have been registered as a customer. Click below to set your password:</p>" +
                           $"<p><a href='{resetLink}'>Set Password</a></p>";

                var response = await _mailHelper.SendEmailAsync(user.Email, "Set your Vitoria Airlines password", body);
                if (!response.IsSuccess)
                {
                    TempData["Error"] = "User created, but email failed.";
                    return RedirectToAction("Index", "Home");
                }

                userId = user.Id;
            }

            var ticket = new Ticket
            {
                FlightId = flightId.Value,
                SeatId = seatId.Value,
                UserId = userId,
                PricePaid = price,
                PurchaseDateUtc = DateTime.UtcNow
            };

            await _ticketRepository.CreateAsync(ticket);

            HttpContext.Session.Remove("FlightId");
            HttpContext.Session.Remove("SeatId");
            HttpContext.Session.Remove("Price");
            HttpContext.Session.Remove("FirstName");
            HttpContext.Session.Remove("LastName");
            HttpContext.Session.Remove("Email");

            return View();
        }



        [HttpGet]
        public IActionResult Cancel()
        {
            return View();
        }


    }
}
