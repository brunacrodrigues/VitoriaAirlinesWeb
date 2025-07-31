using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe.Checkout;
using System.Globalization;
using VitoriaAirlinesWeb.Data.Entities;
using VitoriaAirlinesWeb.Data.Enums;
using VitoriaAirlinesWeb.Data.Repositories;
using VitoriaAirlinesWeb.Helpers;
using VitoriaAirlinesWeb.Models.ViewModels.Booking;
using VitoriaAirlinesWeb.Services;

namespace VitoriaAirlinesWeb.Controllers
{
    /// <summary>
    /// Handles flight booking processes, including seat selection, booking confirmation, payment integration with Stripe,
    /// and managing seat changes for customers.
    /// </summary>
    public class BookingController : Controller
    {
        private readonly IFlightRepository _flightRepository;
        private readonly ITicketRepository _ticketRepository;
        private readonly IConverterHelper _converterHelper;
        private readonly IPaymentService _paymentService;
        private readonly IUserHelper _userHelper;
        private readonly ICustomerProfileRepository _customerProfileRepository;
        private readonly IMailHelper _mailHelper;


        /// <summary>
        /// Initializes a new instance of the BookingController with necessary repositories and helpers.
        /// </summary>
        /// <param name="flightRepository">Repository for flight data access.</param>
        /// <param name="ticketRepository">Repository for ticket data access.</param>
        /// <param name="converterHelper">Helper for converting between entities and view models.</param>
        /// <param name="paymentService">Service for processing payments (e.g., Stripe).</param>
        /// <param name="userHelper">Helper for user-related operations.</param>
        /// <param name="customerProfileRepository">Repository for customer profile data access.</param>
        /// <param name="mailHelper">Helper for sending emails.</param>
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



        /// <summary>
        /// Displays the seat selection interface for a given flight.
        /// Clears previous session booking data for new one-way bookings, validates user roles,
        /// checks flight availability, and identifies already booked seats.
        /// </summary>
        /// <param name="flightId">The ID of the flight for which seats are to be selected.</param>
        /// <param name="ticketId">Optional: The ID of an existing ticket if the user is changing a seat.</param>
        /// <returns>
        /// Task: A view displaying the SelectSeatViewModel, or a redirection to Home/Index with an error, or a NotFoundViewResult.
        /// </returns>
        [HttpGet]
        public async Task<IActionResult> SelectSeat(int flightId, int? ticketId = null)
        {
            // Clear session if this is a fresh booking (not a seat change)
            if (!ticketId.HasValue)
            {
                HttpContext.Session.Remove("OutboundFlightId");
                HttpContext.Session.Remove("OutboundSeatId");
            }

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

            if (User.Identity.IsAuthenticated && User.IsInRole(UserRoles.Customer))
            {
                var user = await _userHelper.GetUserAsync(User);
                if (user != null)
                {
                    var alreadyBooked = await _ticketRepository.UserHasTicketForFlightAsync(user.Id, flightId);
                    if (alreadyBooked && !ticketId.HasValue)
                    {
                        TempData["Error"] = "You have already booked this flight.";
                        return RedirectToAction("Index", "Home");
                    }
                }
            }

            var allTickets = await _ticketRepository.GetTicketsByFlightAsync(flightId);
            var validTickets = allTickets.Where(t => !t.IsCanceled);

            if (ticketId.HasValue)
            {
                validTickets = validTickets.Where(t => t.Id != ticketId.Value);
            }

            var occupiedSeatIds = validTickets.Select(t => t.SeatId).ToList();

            var viewModel = _converterHelper.ToSelectSeatViewModel(flight, occupiedSeatIds);
            viewModel.IsChangingSeat = ticketId.HasValue;
            viewModel.TicketId = ticketId;

            return View(viewModel);
        }



        /// <summary>
        /// Handles the submission of a seat selection for a **one-way flight booking**.
        /// Stores the selected flight and seat in session and proceeds to the confirmation view.
        /// This method is specifically for the flow of a single outbound flight booking.
        /// For round-trip bookings, the flow should typically go through `SelectRoundTripSeats` and its subsequent POST.
        /// </summary>
        /// <param name="flightId">The ID of the selected flight.</param>
        /// <param name="seatId">The ID of the selected seat.</param>
        /// <returns>
        /// Task: A view for confirming a one-way booking, or a redirection to Home/Index with an error.
        /// </returns>
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
                TempData["Error"] = "Flight not found.";
                return RedirectToAction("Index", "Home");
            }

            var seat = flight.Airplane.Seats.FirstOrDefault(s => s.Id == seatId);
            if (seat == null)
            {
                TempData["Error"] = "Seat not found.";
                return RedirectToAction("Index", "Home");
            }

            // Store selection in session for checkout
            HttpContext.Session.SetInt32("FlightId", flightId);
            HttpContext.Session.SetInt32("SeatId", seatId);

            var model = _converterHelper.ToConfirmBookingViewModel(flight, seat);
            model.IsCustomer = User.Identity.IsAuthenticated && User.IsInRole(UserRoles.Customer);

            if (model.IsCustomer)
            {
                var user = await _userHelper.GetUserAsync(User);
                var profile = await _customerProfileRepository.GetByUserIdAsync(user.Id);
                model.ExistingPassportNumber = profile?.PassportNumber;
            }

            return View("ConfirmBooking", model);
        }




        /// <summary>
        /// Initiates a Stripe Checkout Session for a one-way flight booking.
        /// Validates personal data (first name, last name, email, passport number)
        /// based on whether the user is a registered customer or an anonymous guest.
        /// Stores guest data in session for potential user account creation after successful payment.
        /// </summary>
        /// <param name="flightId">The ID of the flight to book.</param>
        /// <param name="seatId">The ID of the selected seat.</param>
        /// <param name="price">The final price of the ticket.</param>
        /// <param name="firstName">First name of the passenger (required for anonymous).</param>
        /// <param name="lastName">Last name of the passenger (required for anonymous).</param>
        /// <param name="email">Email of the passenger (required for anonymous).</param>
        /// <param name="passportNumber">Passport number of the passenger (required for anonymous, or if not in customer profile).</param>
        /// <returns>
        /// Task: Redirects to the Stripe checkout page, or returns the ConfirmBooking view with validation errors.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCheckoutSession(int flightId, int seatId, decimal price, string? firstName, string? lastName, string? email, string? passportNumber)
        {
            // Verificações de segurança e obtenção de dados básicos (voo e assento)
            if (User.Identity.IsAuthenticated && (User.IsInRole(UserRoles.Admin) || User.IsInRole(UserRoles.Employee)))
            {
                TempData["Error"] = "This functionality is reserved for customers only.";
                return RedirectToAction("Index", "Home");
            }

            var flight = await _flightRepository.GetByIdWithAirplaneAndSeatsAsync(flightId);
            if (flight == null)
            {
                TempData["Error"] = "Flight not found."; // Melhor seria retornar View com erro
                return RedirectToAction("Index", "Home");
            }

            var seat = flight.Airplane.Seats.FirstOrDefault(s => s.Id == seatId);
            if (seat == null)
            {
                TempData["Error"] = "Seat not found."; // Melhor seria retornar View com erro
                return RedirectToAction("Index", "Home");
            }

            // Preparar o ViewModel para, potencialmente, retornar à View com erros ou dados preenchidos
            var model = _converterHelper.ToConfirmBookingViewModel(flight, seat);
            model.IsCustomer = User.Identity.IsAuthenticated && User.IsInRole(UserRoles.Customer);
            model.FlightId = flightId; // Assegurar que os IDs estão no modelo
            model.SeatId = seatId;
            model.FinalPrice = price; // Passar o preço também

            // Preencher o modelo com os dados recebidos para re-renderização (se necessário)
            model.FirstName = firstName;
            model.LastName = lastName;
            model.Email = email;
            model.PassportNumber = passportNumber;


            // Lógica de validação condicional para dados pessoais
            if (model.IsCustomer)
            {
                var user = await _userHelper.GetUserAsync(User);
                var profile = await _customerProfileRepository.GetByUserIdAsync(user.Id);

                // VALIDAÇÃO DO PASSAPORTE PARA CLIENTES:
                if (string.IsNullOrWhiteSpace(profile?.PassportNumber))
                {
                    // Se o cliente NÃO tem passaporte registado no perfil: O campo é obrigatório no formulário.
                    if (string.IsNullOrWhiteSpace(passportNumber))
                    {
                        ModelState.AddModelError("PassportNumber", "Passport number is required.");
                    }
                    else
                    {
                        // Se fornecido, verifica se já está em uso por OUTRO utilizador.
                        var existingProfile = await _customerProfileRepository.GetByPassportAsync(passportNumber);
                        if (existingProfile != null && existingProfile.UserId != user.Id)
                        {
                            ModelState.AddModelError("PassportNumber", "This passport number is already in use by another account.");
                        }
                        else
                        {
                            // Se o passaporte é válido e único, atualiza o perfil do cliente
                            profile.PassportNumber = passportNumber;
                            await _customerProfileRepository.UpdateAsync(profile);
                        }
                    }
                }
                else
                {
                    // Se o cliente JÁ tem um passaporte registado no perfil: Usa o do perfil.
                    // Para efeitos de Stripe ou outros, garanta que o 'passportNumber' que vai usar é o do perfil
                    // se o input do form estiver vazio.
                    if (string.IsNullOrWhiteSpace(passportNumber))
                    {
                        passportNumber = profile.PassportNumber;
                        model.PassportNumber = profile.PassportNumber; // Atualiza o modelo para a view
                    }

                }
            }
            else // É um utilizador anónimo
            {
                // VALIDAÇÕES PARA ANÓNIMOS: TODOS os campos são obrigatórios e devem ser únicos (email, passaporte)
                if (string.IsNullOrWhiteSpace(firstName))
                    ModelState.AddModelError("FirstName", "First name is required.");

                if (string.IsNullOrWhiteSpace(lastName))
                    ModelState.AddModelError("LastName", "Last name is required.");

                if (string.IsNullOrWhiteSpace(email))
                    ModelState.AddModelError("Email", "Email is required.");
                else if (!new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(email)) // Valida formato do email
                    ModelState.AddModelError("Email", "Invalid email format.");
                else
                {
                    var existingUser = await _userHelper.GetUserByEmailAsync(email);
                    if (existingUser != null)
                        ModelState.AddModelError("Email", "Invalid email.");
                }

                if (string.IsNullOrWhiteSpace(passportNumber))
                    ModelState.AddModelError("PassportNumber", "Passport number is required.");
                else
                {
                    var existingProfile = await _customerProfileRepository.GetByPassportAsync(passportNumber);
                    if (existingProfile != null)
                        ModelState.AddModelError("PassportNumber", "Invalid passport number.");
                }

                // Se a validação para anónimos passar até aqui, armazena os dados na sessão
                // para serem usados na criação de conta após o pagamento bem-sucedido.

                if (ModelState.IsValid)
                {
                    HttpContext.Session.SetString("FirstName", firstName!);
                    HttpContext.Session.SetString("LastName", lastName!);
                    HttpContext.Session.SetString("Email", email!);
                    HttpContext.Session.SetString("PassportNumber", passportNumber!);
                }
            }

            // VERIFICAÇÃO FINAL DO MODELSTATE
            if (!ModelState.IsValid)
            {
                return View("ConfirmBooking", model);
            }

            HttpContext.Session.SetInt32("FlightId", flightId);
            HttpContext.Session.SetInt32("SeatId", seatId);
            HttpContext.Session.SetString("Price", price.ToString(CultureInfo.InvariantCulture));

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



        /// <summary>
        /// Handles the success callback from Stripe after a single (one-way) booking payment.
        /// Verifies payment status, retrieves booking data from session, creates the ticket,
        /// and for anonymous users, creates a new user account. Finally, sends a confirmation email.
        /// </summary>
        /// <param name="session_id">The Stripe Checkout Session ID provided by Stripe after successful payment.</param>
        /// <returns>
        /// Task: A success view, or a redirection to Home/Index with an error message if payment fails or data is missing/invalid.
        /// </returns>
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
            string email;
            string fullName;

            if (User.Identity.IsAuthenticated && User.IsInRole(UserRoles.Customer))
            {
                var existingUser = await _userHelper.GetUserByEmailAsync(User.Identity.Name);
                if (existingUser == null)
                {
                    TempData["Error"] = "Could not retrieve your user data.";
                    return RedirectToAction("Index", "Home");
                }

                userId = existingUser.Id;
                email = existingUser.Email;
                fullName = existingUser.FullName;
            }
            else
            {
                var sessionEmail = HttpContext.Session.GetString("Email");
                var firstName = HttpContext.Session.GetString("FirstName");
                var lastName = HttpContext.Session.GetString("LastName");
                var passportNumber = HttpContext.Session.GetString("PassportNumber");

                if (string.IsNullOrWhiteSpace(sessionEmail))
                {
                    TempData["Error"] = "Email information is missing.";
                    return RedirectToAction("Index", "Home");
                }

                var newUser = new User
                {
                    UserName = sessionEmail,
                    Email = sessionEmail,
                    FirstName = firstName,
                    LastName = lastName
                };

                var createResult = await _userHelper.AddUserAsync(newUser, "Temp1234!");
                if (!createResult.Succeeded)
                {
                    TempData["Error"] = "User creation failed.";
                    return RedirectToAction("Index", "Home");
                }

                var token = await _userHelper.GenerateEmailConfirmationTokenAsync(newUser);
                await _userHelper.ConfirmEmailAsync(newUser, token);
                await _userHelper.CheckRoleAsync(UserRoles.Customer);
                await _userHelper.AddUserToRoleAsync(newUser, UserRoles.Customer);

                var profile = new CustomerProfile
                {
                    UserId = newUser.Id,
                    PassportNumber = passportNumber
                };
                await _customerProfileRepository.CreateAsync(profile);

                var resetToken = await _userHelper.GeneratePasswordResetTokenAsync(newUser);
                var resetLink = Url.Action("ResetPassword", "Account", new { email = newUser.Email, token = resetToken }, Request.Scheme);
                var body = $"<p>Hello {newUser.FullName},</p>" +
                           $"<p>Thank you for booking with Vitoria Airlines.</p>" +
                           $"<p>You have been registered as a customer. Click below to set your password:</p>" +
                           $"<p><a href='{resetLink}'>Set Password</a></p>";

                await _mailHelper.SendEmailAsync(newUser.Email, "Set your Vitoria Airlines password", body);

                userId = newUser.Id;
                email = newUser.Email;
                fullName = newUser.FullName;
            }

            var conflict = await _ticketRepository.GetBySeatAndFlightAsync(seatId.Value, flightId.Value);
            if (conflict != null)
            {
                TempData["Error"] = "This seat is no longer available.";
                return RedirectToAction("Index", "Home");
            }

            var ticket = new Ticket
            {
                FlightId = flightId.Value,
                SeatId = seatId.Value,
                UserId = userId,
                PricePaid = price,
                PurchaseDateUtc = DateTime.UtcNow
            };

            try
            {
                await _ticketRepository.CreateAsync(ticket);
            }
            catch (DbUpdateException)
            {
                TempData["Error"] = "This seat was already booked. Please choose another.";
                return RedirectToAction("Index", "Home");
            }

            var flight = await _flightRepository.GetByIdWithDetailsAsync(flightId.Value); // Inclui FlightNumber
            var seat = flight.Airplane.Seats.FirstOrDefault(s => s.Id == seatId.Value);   // Inclui Row/Letter

            await _mailHelper.SendBookingConfirmationEmailAsync(
                email,
                fullName,
                flight.FlightNumber,
                $"{seat?.Row}{seat?.Letter}",
                price,
                ticket.PurchaseDateUtc
            );

            HttpContext.Session.Remove("FlightId");
            HttpContext.Session.Remove("SeatId");
            HttpContext.Session.Remove("Price");
            HttpContext.Session.Remove("FirstName");
            HttpContext.Session.Remove("LastName");
            HttpContext.Session.Remove("Email");
            HttpContext.Session.Remove("PassportNumber");

            return View();
        }


        /// <summary>
        /// Displays the payment cancellation page.
        /// This action is typically redirected to when a user cancels the payment process on Stripe.
        /// </summary>
        /// <returns>
        /// IActionResult: The "Cancel" view.
        /// </returns>
        [HttpGet]
        public IActionResult Cancel()
        {
            return View();
        }




        /// <summary>
        /// Displays the confirmation page for a seat change. It fetches the necessary ticket and seat details,
        /// calculates the price difference, and presents this information to the customer for review before proceeding.
        /// Only accessible by authenticated customers.
        /// </summary>
        /// <param name="oldTicketId">The ID of the original ticket whose seat is being changed.</param>
        /// <param name="newSeatId">The ID of the newly selected seat.</param>
        /// <returns>
        /// Task: A view displaying the ConfirmSeatChangeViewModel, or a 404 error/ForbidResult if data is invalid or unauthorized.
        /// </returns>
        [HttpGet]
        [Authorize(Roles = UserRoles.Customer)]
        public async Task<IActionResult> ConfirmSeatChange(int oldTicketId, int newSeatId)
        {
            var oldTicket = await _ticketRepository.GetTicketWithDetailsAsync(oldTicketId);
            if (oldTicket == null) return new NotFoundViewResult("Error404");

            var user = await _userHelper.GetUserByEmailAsync(User.Identity.Name);
            if (oldTicket.UserId != user.Id) return Forbid();

            var flight = oldTicket.Flight;
            var newSeat = flight.Airplane.Seats.FirstOrDefault(s => s.Id == newSeatId);
            if (newSeat == null) return new NotFoundViewResult("Error404");

            var oldPrice = oldTicket.PricePaid;
            var newPrice = (newSeat.Class == SeatClass.Economy) ? flight.EconomyClassPrice : flight.ExecutiveClassPrice;
            var priceDifference = newPrice - oldPrice;


            var viewModel = _converterHelper.ToConfirmSeatChangeViewModel(oldTicket, newSeat, newPrice);

            return View("ConfirmSeatChange", viewModel);
        }



        /// <summary>
        /// Executes the seat change operation. This is a POST action that handles three scenarios
        /// based on the price difference: zero difference, negative difference (refund), and positive
        /// difference (upgrade via Stripe).
        /// </summary>
        /// <param name="oldTicketId">The ID of the original ticket.</param>
        /// <param name="newSeatId">The ID of the newly selected seat.</param>
        /// <returns>
        /// Task: Redirects to MyFlights/Upcoming with a success/error message, or redirects to Stripe for payment.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExecuteSeatChange(int oldTicketId, int newSeatId)
        {
            var ticket = await _ticketRepository.GetTicketWithDetailsAsync(oldTicketId);
            if (ticket == null || ticket.IsCanceled) return new NotFoundViewResult("Error404");

            var user = await _userHelper.GetUserByEmailAsync(User.Identity.Name);
            if (ticket.UserId != user.Id) return Forbid();

            var flight = ticket.Flight;
            var newSeat = flight.Airplane.Seats.FirstOrDefault(s => s.Id == newSeatId);
            if (newSeat == null) return new NotFoundViewResult("Error404");

            if (flight.DepartureUtc <= DateTime.UtcNow.AddHours(24))
            {
                TempData["Error"] = "Seat changes are not allowed less than 24 hours before departure.";
                return RedirectToAction("Upcoming", "MyFlights");
            }

            var oldPrice = ticket.PricePaid;
            var newPrice = (newSeat.Class == SeatClass.Economy) ? flight.EconomyClassPrice : flight.ExecutiveClassPrice;
            var priceDifference = newPrice - oldPrice;

            var seatConflict = await _ticketRepository.GetBySeatAndFlightAsync(newSeatId, flight.Id);
            if (seatConflict != null && seatConflict.Id != ticket.Id && !seatConflict.IsCanceled)
            {
                TempData["Error"] = "This seat is no longer available.";
                return RedirectToAction("Upcoming", "MyFlights");
            }

            if (priceDifference == 0)
            {
                ticket.SeatId = newSeatId;

                try
                {
                    await _ticketRepository.UpdateAsync(ticket);
                }
                catch (DbUpdateException)
                {
                    TempData["Error"] = "Seat change failed due to a conflict. Please try again.";
                    return RedirectToAction("Upcoming", "MyFlights");
                }

                TempData["Success"] = $"Your seat has been successfully changed to {newSeat.Row}{newSeat.Letter} at no extra cost.";
                return RedirectToAction("Upcoming", "MyFlights");
            }
            else if (priceDifference < 0) // downgrade
            {
                var amountToRefund = Math.Abs(priceDifference);
                ticket.SeatId = newSeatId;
                ticket.PricePaid = newPrice;

                try
                {
                    await _ticketRepository.UpdateAsync(ticket);
                }
                catch (DbUpdateException)
                {
                    TempData["Error"] = "Seat change failed due to a conflict. Please try again.";
                    return RedirectToAction("Upcoming", "MyFlights");
                }

                await _mailHelper.SendBookingConfirmationEmailAsync(
                    user.Email,
                    user.FullName,
                    flight.FlightNumber,
                    $"{newSeat.Row}{newSeat.Letter}",
                    newPrice,
                    DateTime.UtcNow
                );

                TempData["Success"] = $"Seat changed to {newSeat.Row}{newSeat.Letter}! A refund of {amountToRefund:C} has been issued.";
                return RedirectToAction("Upcoming", "MyFlights");
            }
            else // priceDifference > 0 - upgrade
            {
                HttpContext.Session.SetInt32("OldTicketId", oldTicketId);
                HttpContext.Session.SetInt32("NewSeatId", newSeatId);
                HttpContext.Session.SetString("PriceDifference", priceDifference.ToString(CultureInfo.InvariantCulture));

                var baseUrl = $"{Request.Scheme}://{Request.Host}";
                var successUrl = $"{baseUrl}/Booking/ChangeSeatSuccess?session_id={{CHECKOUT_SESSION_ID}}";
                var cancelUrl = $"{baseUrl}/Booking/Cancel";

                var description = $"Upgrade from seat {ticket.Seat.Row}{ticket.Seat.Letter} to {newSeat.Row}{newSeat.Letter} on flight {flight.FlightNumber}";

                var checkoutUrl = await _paymentService.CreateSeatUpgradeCheckoutSessionAsync(
                    description,
                    priceDifference,
                    successUrl,
                    cancelUrl
                );

                return Redirect(checkoutUrl);
            }
        }



        /// <summary>
        /// Handles the success callback from Stripe after a seat upgrade payment.
        /// Updates the ticket with the new seat and price, sends a confirmation email, and clears session data.
        /// </summary>
        /// <param name="session_id">The Stripe Checkout Session ID provided by Stripe after successful payment.</param>
        /// <returns>
        /// Task: A success view, or a redirection to MyFlights/Upcoming with an error message.
        /// </returns>
        public async Task<IActionResult> ChangeSeatSuccess(string session_id)
        {
            var sessionService = new SessionService();
            var session = await sessionService.GetAsync(session_id);

            if (session.PaymentStatus != "paid")
            {
                TempData["Error"] = "Payment was not completed.";
                return RedirectToAction("Upcoming", "MyFlights");
            }

            var oldTicketId = HttpContext.Session.GetInt32("OldTicketId");
            var newSeatId = HttpContext.Session.GetInt32("NewSeatId");
            var priceDifferenceString = HttpContext.Session.GetString("PriceDifference");

            if (oldTicketId == null || newSeatId == null || string.IsNullOrEmpty(priceDifferenceString))
            {
                TempData["Error"] = "Critical error: Seat change information was lost after payment. Please contact support immediately.";
                return RedirectToAction("Upcoming", "MyFlights");
            }

            var ticket = await _ticketRepository.GetTicketWithDetailsAsync(oldTicketId.Value);
            if (ticket == null || ticket.IsCanceled)
            {
                TempData["Error"] = "Original ticket could not be found.";
                return RedirectToAction("Upcoming", "MyFlights");
            }

            var newPrice = ticket.PricePaid + decimal.Parse(priceDifferenceString, CultureInfo.InvariantCulture);

            var seatConflict = await _ticketRepository.GetBySeatAndFlightAsync(newSeatId.Value, ticket.FlightId);
            if (seatConflict != null && seatConflict.Id != ticket.Id && !seatConflict.IsCanceled)
            {
                TempData["Error"] = "The selected seat is no longer available.";
                return RedirectToAction("Upcoming", "MyFlights");
            }

            ticket.SeatId = newSeatId.Value;
            ticket.PricePaid = newPrice;

            try
            {
                await _ticketRepository.UpdateAsync(ticket);
            }
            catch (DbUpdateException)
            {
                TempData["Error"] = "Seat change failed due to a conflict. Please try again.";
                return RedirectToAction("Upcoming", "MyFlights");
            }

            // Limpa sessão
            HttpContext.Session.Remove("OldTicketId");
            HttpContext.Session.Remove("NewSeatId");
            HttpContext.Session.Remove("PriceDifference");

            var user = await _userHelper.GetUserByEmailAsync(User.Identity.Name);
            var flight = await _flightRepository.GetByIdWithAirplaneAndSeatsAsync(ticket.FlightId);
            var seat = flight?.Airplane.Seats.FirstOrDefault(s => s.Id == newSeatId.Value);

            if (flight == null || seat == null)
            {
                TempData["Error"] = "Flight or seat details could not be loaded after update.";
                return RedirectToAction("Upcoming", "MyFlights");
            }

            await _mailHelper.SendBookingConfirmationEmailAsync(
                user.Email,
                user.FullName,
                flight.FlightNumber,
                $"{seat.Row}{seat.Letter}",
                ticket.PricePaid,
                DateTime.UtcNow
            );

            return View();
        }



        /// <summary>
        /// Displays the round-trip booking confirmation page.
        /// It retrieves the outbound and return flight/seat selections from the session
        /// and presents them to the user for final review before payment.
        /// </summary>
        /// <returns>
        /// Task: A view with the ConfirmRoundTripViewModel, or a redirection to Home/Index with an error if data is incomplete.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmRoundTrip(int outboundFlightId, int returnFlightId, int outboundSeatId, int returnSeatId)
        {

            var outboundFlight = await _flightRepository.GetByIdWithAirplaneAndSeatsAsync(outboundFlightId);
            var returnFlight = await _flightRepository.GetByIdWithAirplaneAndSeatsAsync(returnFlightId);
            if (outboundFlight == null || returnFlight == null)
            {
                TempData["Error"] = "Flight not found.";
                return RedirectToAction("Index", "Home");
            }

            var outboundSeat = outboundFlight.Airplane.Seats.FirstOrDefault(s => s.Id == outboundSeatId);
            var returnSeat = returnFlight.Airplane.Seats.FirstOrDefault(s => s.Id == returnSeatId);
            if (outboundSeat == null || returnSeat == null)
            {
                TempData["Error"] = "Selected seat not found.";
                return RedirectToAction("Index", "Home");
            }

            var model = new ConfirmRoundTripViewModel
            {
                OutboundFlight = outboundFlight,
                ReturnFlight = returnFlight,
                OutboundSeat = outboundSeat,
                ReturnSeat = returnSeat,
                OutboundFlightId = outboundFlight.Id,
                ReturnFlightId = returnFlight.Id,
                OutboundSeatId = outboundSeat.Id,
                ReturnSeatId = returnSeat.Id,
                IsCustomer = User.Identity.IsAuthenticated && User.IsInRole(UserRoles.Customer)
            };

            if (model.IsCustomer)
            {
                var user = await _userHelper.GetUserAsync(User);
                var profile = await _customerProfileRepository.GetByUserIdAsync(user.Id);
                model.ExistingPassportNumber = profile?.PassportNumber;
            }

            return View(model);
        }



        /// <summary>
        /// Initiates a Stripe Checkout Session for a round-trip flight booking.
        /// Validates personal data (first name, last name, email, passport number)
        /// based on user authentication status (customer vs. anonymous guest).
        /// Stores guest data in session for potential user account creation after successful payment.
        /// </summary>
        /// <param name="outboundFlightId">The ID of the selected outbound flight.</param>
        /// <param name="returnFlightId">The ID of the selected return flight.</param>
        /// <param name="outboundSeatId">The ID of the selected outbound seat.</param>
        /// <param name="returnSeatId">The ID of the selected return seat.</param>
        /// <param name="firstName">First name of the passenger (required for anonymous).</param>
        /// <param name="lastName">Last name of the passenger (required for anonymous).</param>
        /// <param name="email">Email of the passenger (required for anonymous).</param>
        /// <param name="passportNumber">Passport number of the passenger (required for anonymous, or if not in customer profile).</param>
        /// <returns>
        /// Task: Redirects to the Stripe checkout page, or returns the ConfirmRoundTrip view with validation errors.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRoundTripCheckoutSession(int outboundFlightId, int returnFlightId, int outboundSeatId, int returnSeatId, string? firstName, string? lastName, string? email, string? passportNumber)
        {

            var outboundFlight = await _flightRepository.GetByIdWithAirplaneAndSeatsAsync(outboundFlightId);
            var returnFlight = await _flightRepository.GetByIdWithAirplaneAndSeatsAsync(returnFlightId);
            var outboundSeat = outboundFlight?.Airplane.Seats.FirstOrDefault(s => s.Id == outboundSeatId);
            var returnSeat = returnFlight?.Airplane.Seats.FirstOrDefault(s => s.Id == returnSeatId);

            if (outboundFlight == null || returnFlight == null || outboundSeat == null || returnSeat == null)
            {
                ModelState.AddModelError(string.Empty, "Flight or seat data is missing.");
                return RedirectToAction("Index", "Home");
            }

            // Preparar o ViewModel para, potencialmente, retornar à View com erros ou dados preenchidos
            var model = new ConfirmRoundTripViewModel
            {
                OutboundFlightId = outboundFlight.Id,
                ReturnFlightId = returnFlight.Id,
                OutboundSeatId = outboundSeat.Id,
                ReturnSeatId = returnSeat.Id,
                OutboundFlight = outboundFlight,
                ReturnFlight = returnFlight,
                OutboundSeat = outboundSeat,
                ReturnSeat = returnSeat,
                IsCustomer = User.Identity.IsAuthenticated && User.IsInRole(UserRoles.Customer)
            };
            // Preencher o modelo com os dados recebidos para re-renderização (se necessário)
            model.FirstName = firstName;
            model.LastName = lastName;
            model.Email = email;
            model.PassportNumber = passportNumber;


            if (model.IsCustomer)
            {
                var user = await _userHelper.GetUserAsync(User);
                var profile = await _customerProfileRepository.GetByUserIdAsync(user.Id);

                if (string.IsNullOrWhiteSpace(profile?.PassportNumber))
                {
                    if (string.IsNullOrWhiteSpace(passportNumber))
                    {
                        ModelState.AddModelError("PassportNumber", "Passport number is required.");
                    }
                    else
                    {
                        var existingProfile = await _customerProfileRepository.GetByPassportAsync(passportNumber);
                        if (existingProfile != null && existingProfile.UserId != user.Id)
                        {
                            ModelState.AddModelError("PassportNumber", "This passport number is already in use by another account.");
                        }
                        else
                        {
                            profile.PassportNumber = passportNumber;
                            await _customerProfileRepository.UpdateAsync(profile);
                        }
                    }
                }
                else // If the profile already has a passport number, reuse it if none was submitted
                {
                    if (string.IsNullOrWhiteSpace(passportNumber))
                    {
                        passportNumber = profile.PassportNumber;
                        model.PassportNumber = profile.PassportNumber; // Keep model in sync for the view
                    }
                }
            }
            else // É um utilizador anónimo
            {
                if (string.IsNullOrWhiteSpace(firstName))
                    ModelState.AddModelError("FirstName", "First name is required.");

                if (string.IsNullOrWhiteSpace(lastName))
                    ModelState.AddModelError("LastName", "Last name is required.");

                if (string.IsNullOrWhiteSpace(email))
                    ModelState.AddModelError("Email", "Email is required.");
                else if (!new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(email))
                    ModelState.AddModelError("Email", "Invalid email format.");
                else
                {
                    var existingUser = await _userHelper.GetUserByEmailAsync(email);
                    if (existingUser != null)
                        ModelState.AddModelError("Email", "Invalid email.");
                }

                if (string.IsNullOrWhiteSpace(passportNumber))
                    ModelState.AddModelError("PassportNumber", "Passport number is required.");
                else
                {
                    var existingProfile = await _customerProfileRepository.GetByPassportAsync(passportNumber);
                    if (existingProfile != null)
                        ModelState.AddModelError("PassportNumber", "Invalid passport number.");
                }

                if (ModelState.IsValid)
                {
                    HttpContext.Session.SetString("FirstName", firstName!);
                    HttpContext.Session.SetString("LastName", lastName!);
                    HttpContext.Session.SetString("Email", email!);
                    HttpContext.Session.SetString("PassportNumber", passportNumber!);
                }
            }

            if (!ModelState.IsValid)
            {
                return View("ConfirmRoundTrip", model);
            }

            // Se tudo válido, procede com a criação da sessão de pagamento
            var priceOutbound = model.OutboundPrice;
            var priceReturn = model.ReturnPrice;
            var totalPrice = model.TotalPrice;


            HttpContext.Session.SetInt32("OutboundFlightId", outboundFlightId);
            HttpContext.Session.SetInt32("ReturnFlightId", returnFlightId);
            HttpContext.Session.SetInt32("OutboundSeatId", outboundSeatId);
            HttpContext.Session.SetInt32("ReturnSeatId", returnSeatId);
            HttpContext.Session.SetString("OutboundPrice", priceOutbound.ToString(CultureInfo.InvariantCulture));
            HttpContext.Session.SetString("ReturnPrice", priceReturn.ToString(CultureInfo.InvariantCulture));


            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var successUrl = $"{baseUrl}/Booking/RoundTripSuccess?session_id={{CHECKOUT_SESSION_ID}}";
            var cancelUrl = $"{baseUrl}/Booking/Cancel";

            var checkoutUrl = await _paymentService.CreateRoundTripCheckoutSessionAsync(
                outboundFlight,
                returnFlight,
                outboundSeat,
                returnSeat,
                priceOutbound,
                priceReturn,
                successUrl,
                cancelUrl
            );

            return Redirect(checkoutUrl);
        }



        /// <summary>
        /// Handles the success callback from Stripe after a round-trip booking payment.
        /// Verifies payment status, retrieves booking data from session, creates both outbound and return tickets,
        /// and for anonymous users, creates a new user account. Finally, sends confirmation emails.
        /// </summary>
        /// <param name="session_id">The Stripe Checkout Session ID provided by Stripe after successful payment.</param>
        /// <returns>
        /// Task: A success view, or a redirection to Home/Index with an error message if payment fails or data is missing/invalid.
        /// </returns>
        public async Task<IActionResult> RoundTripSuccess(string session_id)
        {
            var sessionService = new SessionService();
            var session = await sessionService.GetAsync(session_id);

            if (session.PaymentStatus != "paid")
            {
                TempData["Error"] = "Payment was not completed.";
                return RedirectToAction("Index", "Home");
            }

            var outboundFlightId = HttpContext.Session.GetInt32("OutboundFlightId");
            var returnFlightId = HttpContext.Session.GetInt32("ReturnFlightId");
            var outboundSeatId = HttpContext.Session.GetInt32("OutboundSeatId");
            var returnSeatId = HttpContext.Session.GetInt32("ReturnSeatId");
            var outboundPriceStr = HttpContext.Session.GetString("OutboundPrice");
            var returnPriceStr = HttpContext.Session.GetString("ReturnPrice");

            if (outboundFlightId == null || returnFlightId == null || outboundSeatId == null || returnSeatId == null ||
                string.IsNullOrEmpty(outboundPriceStr) || string.IsNullOrEmpty(returnPriceStr))
            {
                TempData["Error"] = "Booking data is missing.";
                return RedirectToAction("Index", "Home");
            }

            decimal outboundPrice = decimal.Parse(outboundPriceStr, CultureInfo.InvariantCulture);
            decimal returnPrice = decimal.Parse(returnPriceStr, CultureInfo.InvariantCulture);

            var outboundFlight = await _flightRepository.GetByIdWithAirplaneAndSeatsAsync(outboundFlightId.Value);
            var returnFlight = await _flightRepository.GetByIdWithAirplaneAndSeatsAsync(returnFlightId.Value);

            if (outboundFlight == null || returnFlight == null)
            {
                TempData["Error"] = "Flight data missing.";
                return RedirectToAction("Index", "Home");
            }

            var outboundSeat = outboundFlight.Airplane.Seats.FirstOrDefault(s => s.Id == outboundSeatId.Value);
            var returnSeat = returnFlight.Airplane.Seats.FirstOrDefault(s => s.Id == returnSeatId.Value);

            if (outboundSeat == null || returnSeat == null)
            {
                TempData["Error"] = "Seat data missing.";
                return RedirectToAction("Index", "Home");
            }

            string userId;
            string email;
            string fullName;

            if (User.Identity.IsAuthenticated && User.IsInRole(UserRoles.Customer))
            {
                var user = await _userHelper.GetUserByEmailAsync(User.Identity.Name);
                if (user == null)
                {
                    TempData["Error"] = "Could not retrieve user data.";
                    return RedirectToAction("Index", "Home");
                }

                userId = user.Id;
                email = user.Email;
                fullName = user.FullName;
            }
            else // é anonimo 
            {
                var emailSession = HttpContext.Session.GetString("Email");
                var firstName = HttpContext.Session.GetString("FirstName");
                var lastName = HttpContext.Session.GetString("LastName");
                var passportNumber = HttpContext.Session.GetString("PassportNumber");

                if (string.IsNullOrWhiteSpace(emailSession))
                {
                    TempData["Error"] = "Missing user email.";
                    return RedirectToAction("Index", "Home");
                }

                var newUser = new User
                {
                    UserName = emailSession,
                    Email = emailSession,
                    FirstName = firstName,
                    LastName = lastName
                };

                var result = await _userHelper.AddUserAsync(newUser, "Temp1234!");
                if (!result.Succeeded)
                {
                    TempData["Error"] = "User creation failed.";
                    return RedirectToAction("Index", "Home");
                }

                var token = await _userHelper.GenerateEmailConfirmationTokenAsync(newUser);
                await _userHelper.ConfirmEmailAsync(newUser, token);
                await _userHelper.CheckRoleAsync(UserRoles.Customer);
                await _userHelper.AddUserToRoleAsync(newUser, UserRoles.Customer);

                var profile = new CustomerProfile
                {
                    UserId = newUser.Id,
                    PassportNumber = passportNumber
                };
                await _customerProfileRepository.CreateAsync(profile);

                var resetToken = await _userHelper.GeneratePasswordResetTokenAsync(newUser);
                var resetLink = Url.Action("ResetPassword", "Account", new { email = newUser.Email, token = resetToken }, Request.Scheme);
                var body = $"<p>Hello {newUser.FullName},</p>" +
                           $"<p>Thank you for booking with Vitoria Airlines.</p>" +
                           $"<p>You have been registered as a customer. Click below to set your password:</p>" +
                           $"<p><a href='{resetLink}'>Set Password</a></p>";

                await _mailHelper.SendEmailAsync(newUser.Email, "Set your Vitoria Airlines password", body);

                userId = newUser.Id;
                email = newUser.Email;
                fullName = newUser.FullName;
            }

            var ticket1Exists = await _ticketRepository.UserHasTicketForFlightAsync(userId, outboundFlightId.Value);
            var ticket2Exists = await _ticketRepository.UserHasTicketForFlightAsync(userId, returnFlightId.Value);
            if (ticket1Exists || ticket2Exists)
            {
                TempData["Error"] = "You already have a ticket for one of the selected flights.";
                return RedirectToAction("Index", "Home");
            }

            var outboundConflict = await _ticketRepository.GetBySeatAndFlightAsync(outboundSeatId.Value, outboundFlightId.Value);
            var returnConflict = await _ticketRepository.GetBySeatAndFlightAsync(returnSeatId.Value, returnFlightId.Value);

            if ((outboundConflict != null && !outboundConflict.IsCanceled) || (returnConflict != null && !returnConflict.IsCanceled))
            {
                TempData["Error"] = "One or both of the selected seats have already been taken.";
                return RedirectToAction("Index", "Home");
            }

            var ticketOutbound = new Ticket
            {
                FlightId = outboundFlight.Id,
                SeatId = outboundSeat.Id,
                UserId = userId,
                PricePaid = outboundPrice,
                PurchaseDateUtc = DateTime.UtcNow
            };

            var ticketReturn = new Ticket
            {
                FlightId = returnFlight.Id,
                SeatId = returnSeat.Id,
                UserId = userId,
                PricePaid = returnPrice,
                PurchaseDateUtc = DateTime.UtcNow
            };

            try
            {
                await _ticketRepository.CreateAsync(ticketOutbound);
                await _ticketRepository.CreateAsync(ticketReturn);
            }
            catch (DbUpdateException)
            {
                TempData["Error"] = "Seat conflict occurred during booking. Please try again.";
                return RedirectToAction("Index", "Home");
            }

            await _mailHelper.SendBookingConfirmationEmailAsync(
                email,
                fullName,
                outboundFlight.FlightNumber,
                $"{outboundSeat.Row}{outboundSeat.Letter}",
                outboundPrice,
                ticketOutbound.PurchaseDateUtc
            );

            await _mailHelper.SendBookingConfirmationEmailAsync(
                email,
                fullName,
                returnFlight.FlightNumber,
                $"{returnSeat.Row}{returnSeat.Letter}",
                returnPrice,
                ticketReturn.PurchaseDateUtc
            );

            HttpContext.Session.Remove("OutboundFlightId");
            HttpContext.Session.Remove("ReturnFlightId");
            HttpContext.Session.Remove("OutboundSeatId");
            HttpContext.Session.Remove("ReturnSeatId");
            HttpContext.Session.Remove("OutboundPrice");
            HttpContext.Session.Remove("ReturnPrice");
            HttpContext.Session.Remove("FirstName");
            HttpContext.Session.Remove("LastName");
            HttpContext.Session.Remove("Email");
            HttpContext.Session.Remove("PassportNumber");

            return View("Success");
        }



        /// <summary>
        /// Displays the view for selecting seats for both outbound and return flights of a round trip.
        /// This action is typically accessed after the user searches for round-trip flights and selects them.
        /// </summary>
        /// <param name="outboundFlightId">The ID of the selected outbound flight.</param>
        /// <param name="returnFlightId">The ID of the selected return flight.</param>
        /// <returns>
        /// Task: A view with the SelectRoundTripSeatViewModel, or a redirection to Home/Index with an error.
        /// </returns>

        [HttpGet]
        public async Task<IActionResult> SelectRoundTripSeats(int outboundFlightId, int returnFlightId)
        {
            if (User.Identity.IsAuthenticated && (User.IsInRole(UserRoles.Admin) || User.IsInRole(UserRoles.Employee)))
            {
                TempData["Error"] = "This functionality is reserved for customers only.";
                return RedirectToAction("Index", "Home");
            }

            var outboundFlight = await _flightRepository.GetByIdWithAirplaneAndSeatsAsync(outboundFlightId);
            var returnFlight = await _flightRepository.GetByIdWithAirplaneAndSeatsAsync(returnFlightId);

            if (outboundFlight == null || outboundFlight.Status != FlightStatus.Scheduled ||
                returnFlight == null || returnFlight.Status != FlightStatus.Scheduled)
            {
                TempData["Error"] = "One or both of the selected flights are not available for booking.";
                return RedirectToAction("Index", "Home");
            }

            if (User.Identity.IsAuthenticated && User.IsInRole(UserRoles.Customer))
            {
                var user = await _userHelper.GetUserAsync(User);
                if (user != null)
                {
                    var alreadyBookedOutbound = await _ticketRepository.UserHasTicketForFlightAsync(user.Id, outboundFlightId);
                    var alreadyBookedReturn = await _ticketRepository.UserHasTicketForFlightAsync(user.Id, returnFlightId);

                    if (alreadyBookedOutbound || alreadyBookedReturn)
                    {
                        TempData["Error"] = "You have already booked one of the selected flights.";
                        return RedirectToAction("Index", "Home");
                    }
                }
            }

            var outboundTickets = await _ticketRepository.GetTicketsByFlightAsync(outboundFlightId);
            var returnTickets = await _ticketRepository.GetTicketsByFlightAsync(returnFlightId);

            var validOutboundTickets = outboundTickets.Where(t => !t.IsCanceled);
            var validReturnTickets = returnTickets.Where(t => !t.IsCanceled);

            var occupiedOutbound = validOutboundTickets.Select(t => t.SeatId).ToList();
            var occupiedReturn = validReturnTickets.Select(t => t.SeatId).ToList();

            var model = new SelectRoundTripSeatViewModel
            {
                Outbound = _converterHelper.ToSelectSeatViewModel(outboundFlight, occupiedOutbound),
                Return = _converterHelper.ToSelectSeatViewModel(returnFlight, occupiedReturn)
            };

            return View("SelectRoundTripSeats", model);
        }


    }
}
