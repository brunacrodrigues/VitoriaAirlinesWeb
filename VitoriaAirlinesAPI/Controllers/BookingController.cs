using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Stripe;
using Stripe.Checkout;
using System.Security.Claims;
using System.Text;
using VitoriaAirlinesAPI.Dtos;
using VitoriaAirlinesAPI.Services;
using VitoriaAirlinesWeb.Data;
using VitoriaAirlinesWeb.Data.Entities;
using VitoriaAirlinesWeb.Data.Enums;
using VitoriaAirlinesWeb.Data.Repositories;
using VitoriaAirlinesWeb.Helpers;
using VitoriaAirlinesWeb.Services;

namespace VitoriaAirlinesAPI.Controllers
{
    [Authorize(
    AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
    Roles = UserRoles.Customer)]
    [Route("api/[controller]")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        private readonly IUserHelper _userHelper;
        private readonly IFlightRepository _flightRepository;
        private readonly ITicketRepository _ticketRepository;
        private readonly ICustomerProfileRepository _customerProfileRepository;
        private readonly IPaymentService _paymentService;
        private readonly ITicketPdfService _ticketPdfService;
        private readonly IMailHelper _mailHelper;

        //private const string ApiBaseUrl = "http://localhost:5283";
        private const string ApiBaseUrl = "http://10.0.2.2:5283";
        //private const string ApiBaseUrl = "http://192.168.1.254:5283";
        //private const string ApiBaseUrl = "http://vitoriaairlinesapi.eu-north-1.elasticbeanstalk.com";
        private const string AppDeepLink = "vitoriaairlines://app/booking/success";
        private const string WebAppUrl = "https://localhost:7106";

        public BookingController(
            IUserHelper userHelper,
            IFlightRepository flightRepository,
            ITicketRepository ticketRepository,
            ICustomerProfileRepository customerProfileRepository,
            IPaymentService paymentService,
            ITicketPdfService ticketPdfService,
            IMailHelper mailHelper)
        {
            _userHelper = userHelper;
            _flightRepository = flightRepository;
            _ticketRepository = ticketRepository;
            _customerProfileRepository = customerProfileRepository;
            _paymentService = paymentService;
            _ticketPdfService = ticketPdfService;
            _mailHelper = mailHelper;
        }


        [HttpPost("create-checkout-session")]
        [AllowAnonymous]
        public async Task<IActionResult> CreateCheckoutSession([FromBody] BookingRequestDto request)
        {
            var metadata = new Dictionary<string, string>();


            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userEmailClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            bool isUserLoggedIn = !string.IsNullOrWhiteSpace(userId) || !string.IsNullOrWhiteSpace(userEmailClaim);


            if (isUserLoggedIn)
            {

                if (string.IsNullOrWhiteSpace(userId) && !string.IsNullOrWhiteSpace(userEmailClaim))
                {
                    var userByEmail = await _userHelper.GetUserByEmailAsync(userEmailClaim);
                    userId = userByEmail?.Id;
                }


                if (string.IsNullOrWhiteSpace(userId))
                {
                    return Unauthorized("Could not identify the user from the token claims.");
                }



                metadata["IsAnonymous"] = "false";
                metadata["UserId"] = userId;


                var currentProfile = await _customerProfileRepository.GetByUserIdAsync(userId);


                if (!string.IsNullOrWhiteSpace(currentProfile?.PassportNumber))
                {
                    if (request.PassportNumber != currentProfile.PassportNumber)
                    {
                        return BadRequest("Your registered passport number cannot be changed. Please ensure the entered number matches your profile.");
                    }
                }
                else
                {

                    var existingProfile = await _customerProfileRepository.GetByPassportAsync(request.PassportNumber);
                    if (existingProfile != null && existingProfile.UserId != userId)
                    {
                        return BadRequest("The passport number entered is already associated with another customer account.");
                    }
                }

                metadata["FirstName"] = request.FirstName;
                metadata["LastName"] = request.LastName;
                metadata["Email"] = request.Email;
                metadata["PassportNumber"] = request.PassportNumber;
            }
            else
            {

                if (string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.LastName) ||
                    string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.PassportNumber))
                {
                    return BadRequest("For anonymous booking, all personal details are required.");
                }

                if (await _userHelper.GetUserByEmailAsync(request.Email) != null || await _customerProfileRepository.GetByPassportAsync(request.PassportNumber) != null)
                {
                    return BadRequest("Email or passport is already associated with an account. Please log in.");
                }


                metadata["IsAnonymous"] = "true";
                metadata["FirstName"] = request.FirstName;
                metadata["LastName"] = request.LastName;
                metadata["Email"] = request.Email;
                metadata["PassportNumber"] = request.PassportNumber;
            }


            foreach (var leg in request.Legs)
            {
                var flight = await _flightRepository.GetByIdWithAirplaneAndSeatsAsync(leg.FlightId);
                if (flight == null || flight.Status != FlightStatus.Scheduled)
                {
                    return BadRequest($"Flight ID {leg.FlightId} is not available for booking.");
                }
                if (flight.Airplane.Seats.All(s => s.Id != leg.SeatId))
                {
                    return BadRequest($"Seat ID {leg.SeatId} is invalid for the selected flight.");
                }
                var seatIsTaken = await _ticketRepository.GetBySeatAndFlightAsync(leg.SeatId, leg.FlightId);
                if (seatIsTaken != null)
                {
                    return BadRequest($"The selected seat on flight {flight.FlightNumber} is no longer available.");
                }
            }

            metadata["OutboundFlightId"] = request.Legs[0].FlightId.ToString();
            metadata["OutboundSeatId"] = request.Legs[0].SeatId.ToString();

            if (request.Legs.Count > 1)
            {
                metadata["ReturnFlightId"] = request.Legs[1].FlightId.ToString();
                metadata["ReturnSeatId"] = request.Legs[1].SeatId.ToString();
            }


            var successUrl = $"{ApiBaseUrl}/api/booking/payment-success-redirect?session_id={{CHECKOUT_SESSION_ID}}";
            var cancelUrl = $"{ApiBaseUrl}/api/booking/payment-cancel";

            try
            {
                var session = await _paymentService.CreateApiCheckoutSessionAsync(request, metadata, successUrl, cancelUrl);
                return Ok(new CreateCheckoutResponseDto { CheckoutUrl = session.Url, StripeSessionId = session.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while creating the payment session: {ex.Message}");
            }
        }



        [AllowAnonymous]
        [HttpGet("payment-success-redirect")]
        public IActionResult PaymentSuccessRedirect(string session_id)
        {
            var appDeepLink = $"{AppDeepLink}?sessionId={session_id}";
            return Redirect(appDeepLink);
        }


        [HttpPost("complete-booking")]
        [AllowAnonymous]
        public async Task<IActionResult> CompleteBooking([FromBody] CompleteBookingRequestDto request)
        {
            var sessionService = new SessionService();
            Session session;
            try
            {
                session = await sessionService.GetAsync(request.StripeSessionId);
            }
            catch (StripeException)
            {
                return BadRequest("Invalid Stripe session ID.");
            }

            if (session.PaymentStatus != "paid")
            {
                return BadRequest("Payment has not been completed for this session.");
            }

            var metadata = session.Metadata;
            var isAnonymous = bool.Parse(metadata["IsAnonymous"]);

            string userId;
            string userEmail;
            string userFullName;


            if (isAnonymous)
            {
                var email = metadata["Email"];
                var firstName = metadata["FirstName"];
                var lastName = metadata["LastName"];
                var passport = metadata["PassportNumber"];

                var newUser = new User
                {
                    UserName = email,
                    Email = email,
                    FirstName = firstName,
                    LastName = lastName
                };


                var result = await _userHelper.AddUserAsync(newUser, $"Temp{Guid.NewGuid().ToString().Substring(0, 8)}!");
                if (!result.Succeeded)
                {
                    return StatusCode(500, "Failed to create user account after payment.");
                }

                await _userHelper.CheckRoleAsync(UserRoles.Customer);
                await _userHelper.AddUserToRoleAsync(newUser, UserRoles.Customer);
                var token = await _userHelper.GenerateEmailConfirmationTokenAsync(newUser);
                await _userHelper.ConfirmEmailAsync(newUser, token);

                var profile = new CustomerProfile { UserId = newUser.Id, PassportNumber = passport };
                await _customerProfileRepository.CreateAsync(profile);




                var resetToken = await _userHelper.GeneratePasswordResetTokenAsync(newUser);

                var tokenBytes = Encoding.UTF8.GetBytes(resetToken);
                var base64Token = WebEncoders.Base64UrlEncode(tokenBytes);

                var resetLink = $"{WebAppUrl}/Account/ResetPassword?token={base64Token}&email={Uri.EscapeDataString(email)}";


                var body = $"<p>Hello {newUser.FullName},</p>" +
                           $"<p>You have been registered as a customer. Click below to set your password:</p>" +
                           $"<p><a href='{resetLink}'>Set Password</a></p>";
                await _mailHelper.SendEmailAsync(email, "Welcome to Vitoria Airlines - Set Your Password", body);

                userId = newUser.Id;
                userEmail = email;
                userFullName = newUser.FullName;
            }
            else
            {
                userId = metadata["UserId"];
                var user = await _userHelper.GetUserByIdAsync(userId);
                if (user == null) return Unauthorized("User not found.");
                userEmail = user.Email;
                userFullName = user.FullName;
            }



            var outboundFlightId = int.Parse(metadata["OutboundFlightId"]);
            var outboundSeatId = int.Parse(metadata["OutboundSeatId"]);

            if (await _ticketRepository.GetBySeatAndFlightAsync(outboundSeatId, outboundFlightId) != null)
            {
                return StatusCode(500, "Outbound seat was taken during payment process. Please contact support for refund.");
            }

            var outboundFlight = await _flightRepository.GetByIdWithAirplaneAndSeatsAsync(outboundFlightId);
            var outboundSeat = outboundFlight.Airplane.Seats.First(s => s.Id == outboundSeatId);
            var outboundPrice = outboundSeat.Class == SeatClass.Economy ? outboundFlight.EconomyClassPrice : outboundFlight.ExecutiveClassPrice;

            var outboundTicket = new Ticket
            {
                FlightId = outboundFlightId,
                SeatId = outboundSeatId,
                UserId = userId,
                PricePaid = outboundPrice,
                PurchaseDateUtc = DateTime.UtcNow
            };
            await _ticketRepository.CreateAsync(outboundTicket);


            var outboundTicketWithDetails = await _ticketRepository.GetTicketWithDetailsAsync(outboundTicket.Id);
            var outboundPdfBytes = await _ticketPdfService.GenerateTicketPdfAsync(outboundTicketWithDetails);


            await _mailHelper.SendBookingConfirmationEmailWithAttachmentAsync(
                userEmail,
                userFullName,
                outboundFlight.FlightNumber,
                $"{outboundSeat.Row}{outboundSeat.Letter}",
                outboundPrice,
                outboundTicket.PurchaseDateUtc,
                outboundPdfBytes,
                $"BoardingPass_TKT{outboundTicket.Id:D8}.pdf"
            );

            int? returnTicketId = null;


            if (metadata.ContainsKey("ReturnFlightId"))
            {
                var returnFlightId = int.Parse(metadata["ReturnFlightId"]);
                var returnSeatId = int.Parse(metadata["ReturnSeatId"]);

                if (await _ticketRepository.GetBySeatAndFlightAsync(returnSeatId, returnFlightId) != null)
                {
                    return StatusCode(500, "Return seat was taken during payment process. Please contact support.");
                }

                var returnFlight = await _flightRepository.GetByIdWithAirplaneAndSeatsAsync(returnFlightId);
                var returnSeat = returnFlight.Airplane.Seats.First(s => s.Id == returnSeatId);
                var returnPrice = returnSeat.Class == SeatClass.Economy ? returnFlight.EconomyClassPrice : returnFlight.ExecutiveClassPrice;

                var returnTicket = new Ticket
                {
                    FlightId = returnFlightId,
                    SeatId = returnSeatId,
                    UserId = userId,
                    PricePaid = returnPrice,
                    PurchaseDateUtc = DateTime.UtcNow
                };
                await _ticketRepository.CreateAsync(returnTicket);


                var returnTicketWithDetails = await _ticketRepository.GetTicketWithDetailsAsync(returnTicket.Id);
                var returnPdfBytes = await _ticketPdfService.GenerateTicketPdfAsync(returnTicketWithDetails);


                await _mailHelper.SendBookingConfirmationEmailWithAttachmentAsync(
                    userEmail,
                    userFullName,
                    returnFlight.FlightNumber,
                    $"{returnSeat.Row}{returnSeat.Letter}",
                    returnPrice,
                    returnTicket.PurchaseDateUtc,
                    returnPdfBytes,
                    $"BoardingPass_TKT{returnTicket.Id:D8}.pdf"
                );

                returnTicketId = returnTicket.Id;
            }


            return Ok(new CompleteBookingResponseDto { OutboundTicketId = outboundTicket.Id, ReturnTicketId = returnTicketId });
        }


        [HttpPost("{ticketId}/cancel")]
        public async Task<IActionResult> CancelTicket(int ticketId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();


            var ticket = await _ticketRepository.GetTicketWithDetailsAsync(ticketId);

            if (ticket == null || ticket.UserId != userId || ticket.IsCanceled)
            {
                return Forbid("Ticket not found, already canceled, or does not belong to the user.");
            }


            const int CancellationCutoffHours = 24;
            if (ticket.Flight.DepartureUtc.Subtract(DateTime.UtcNow).TotalHours < CancellationCutoffHours)
            {
                return BadRequest($"Cancellation must be done at least {CancellationCutoffHours} hours before departure.");
            }

            ticket.IsCanceled = true;
            ticket.CanceledDateUtc = DateTime.UtcNow;

            await _ticketRepository.UpdateAsync(ticket);

            try
            {
                var user = await _userHelper.GetUserByIdAsync(userId);
                if (user == null) throw new Exception("User data missing for email.");

                var body = $@"<p>Hello {user.FullName},</p>
                         <p>Your ticket for flight <strong>{ticket.Flight.FlightNumber}</strong> scheduled on 
                         <strong>{ticket.Flight.DepartureUtc:dd/MM/yyyy HH:mm} (UTC)</strong>
                         has been successfully canceled.</p>
                         <p>A refund will be issued shortly to the payment method used.</p>
                         <p>Thank you,<br/>Vitoria Airlines</p>";

                await _mailHelper.SendEmailAsync(user.Email, "Ticket Cancellation Confirmation", body);
            }
            catch (Exception ex)
            {
                return Ok(new { message = $"Ticket {ticketId} successfully canceled. WARNING: Failed to send confirmation email." });
            }

            return Ok(new { message = "Ticket successfully canceled and refund confirmation sent." });
        }

    }
}