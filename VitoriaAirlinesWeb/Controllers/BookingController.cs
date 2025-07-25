using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        public async Task<IActionResult> SelectSeat(int flightId, int? ticketId = null)
        {
            // Limpa sessão de voos se não estiver a alterar assento
            // Só limpa se estivermos num voo isolado e não houver volta prevista
            if (!ticketId.HasValue && HttpContext.Session.GetInt32("ReturnFlightId") == null)
            {
                HttpContext.Session.Remove("OutboundFlightId");
                HttpContext.Session.Remove("OutboundSeatId");
                HttpContext.Session.Remove("ReturnFlightId");
                HttpContext.Session.Remove("ReturnSeatId");
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

            var occupiedSeatIds = validTickets
                .Select(t => t.SeatId)
                .ToList();



            var viewModel = _converterHelper.ToSelectSeatViewModelAsync(flight, occupiedSeatIds);

            viewModel.IsChangingSeat = ticketId.HasValue;
            viewModel.TicketId = ticketId;

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
                TempData["Error"] = "Flight not found.";
                return RedirectToAction("Index", "Home");
            }

            var seat = flight.Airplane.Seats.FirstOrDefault(s => s.Id == seatId);
            if (seat == null)
            {
                TempData["Error"] = "Seat not found.";
                return RedirectToAction("Index", "Home");
            }

            // Se ainda não temos ida registada na sessão, tratamos como voo de ida
            if (HttpContext.Session.GetInt32("OutboundFlightId") == null)
            {
                HttpContext.Session.SetInt32("OutboundFlightId", flightId);
                HttpContext.Session.SetInt32("OutboundSeatId", seatId);

                var returnFlightId = HttpContext.Session.GetInt32("ReturnFlightId");

                if (returnFlightId.HasValue)
                {
                    // Round-trip: ir selecionar o lugar da volta
                    return RedirectToAction("SelectSeat", new { flightId = returnFlightId.Value });
                }
                else
                {
                    // One-way: ir direto para confirmação da compra
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
            }

            HttpContext.Session.SetInt32("ReturnFlightId", flightId);
            HttpContext.Session.SetInt32("ReturnSeatId", seatId);

            return RedirectToAction("ConfirmRoundTrip");
        }


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
                        ModelState.AddModelError("Email", "An account with this email already exists.");
                }

                if (string.IsNullOrWhiteSpace(passportNumber))
                    ModelState.AddModelError("PassportNumber", "Passport number is required.");
                else
                {
                    var existingProfile = await _customerProfileRepository.GetByPassportAsync(passportNumber);
                    if (existingProfile != null)
                        ModelState.AddModelError("PassportNumber", "This passport number is already in use.");
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
                var passportNumber = HttpContext.Session.GetString("PassportNumber");


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
                    UserId = user.Id,
                    PassportNumber = passportNumber
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

            var alreadyBooked = await _ticketRepository.UserHasTicketForFlightAsync(userId, flightId.Value);
            if (alreadyBooked)
            {
                TempData["Error"] = "You already have a ticket for this flight.";
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

            await _ticketRepository.CreateAsync(ticket);

            HttpContext.Session.Remove("FlightId");
            HttpContext.Session.Remove("SeatId");
            HttpContext.Session.Remove("Price");
            HttpContext.Session.Remove("FirstName");
            HttpContext.Session.Remove("LastName");
            HttpContext.Session.Remove("Email");
            HttpContext.Session.Remove("PassportNumber");


            return View();
        }



        [HttpGet]
        public IActionResult Cancel()
        {
            return View();
        }



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

            if (ticket.Flight.DepartureUtc <= DateTime.UtcNow.AddHours(24))
            {
                TempData["Error"] = "Seat changes are not allowed less than 24 hours before departure.";
                return RedirectToAction("Upcoming", "MyFlights");
            }


            var oldPrice = ticket.PricePaid;
            var newPrice = (newSeat.Class == SeatClass.Economy) ? flight.EconomyClassPrice : flight.ExecutiveClassPrice;
            var priceDifference = newPrice - oldPrice;

            if (priceDifference == 0)
            {
                ticket.SeatId = newSeatId;
                await _ticketRepository.UpdateAsync(ticket);

                TempData["Success"] = $"Your seat has been successfully changed to {newSeat.Row}{newSeat.Letter} at no extra cost.";
                return RedirectToAction("Upcoming", "MyFlights");
            }

            else if (priceDifference < 0)
            {
                var amountToRefund = Math.Abs(priceDifference);
                ticket.SeatId = newSeatId;
                ticket.PricePaid = newPrice;
                await _ticketRepository.UpdateAsync(ticket);

                // TODO send email

                TempData["Success"] = $"Seat changed to {newSeat.Row}{newSeat.Letter}! A refund of {amountToRefund:C} has been issued.";
                return RedirectToAction("Upcoming", "MyFlights");
            }

            else
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

            if (oldTicketId == null || newSeatId == null)
            {
                TempData["Error"] = "Critical error: Seat change information was lost after payment. Please contact support immediately.";
                return RedirectToAction("Upcoming", "MyFlights");
            }

            var ticket = await _ticketRepository.GetByIdAsync(oldTicketId.Value);
            var newPrice = ticket.PricePaid + decimal.Parse(priceDifferenceString, CultureInfo.InvariantCulture);

            ticket.SeatId = newSeatId.Value;
            ticket.PricePaid = newPrice;

            await _ticketRepository.UpdateAsync(ticket);

            HttpContext.Session.Remove("OldTicketId");
            HttpContext.Session.Remove("NewSeatId");
            HttpContext.Session.Remove("PriceDifference");

            return View();
        }



        [HttpGet]
        public async Task<IActionResult> ConfirmRoundTrip()
        {
            var outboundFlightId = HttpContext.Session.GetInt32("OutboundFlightId");
            var returnFlightId = HttpContext.Session.GetInt32("ReturnFlightId");
            var outboundSeatId = HttpContext.Session.GetInt32("OutboundSeatId");
            var returnSeatId = HttpContext.Session.GetInt32("ReturnSeatId");

            if (outboundFlightId == null || returnFlightId == null || outboundSeatId == null || returnSeatId == null)
            {
                TempData["Error"] = "Booking information is incomplete.";
                return RedirectToAction("Index", "Home");
            }

            var outboundFlight = await _flightRepository.GetByIdWithAirplaneAndSeatsAsync(outboundFlightId.Value);
            var returnFlight = await _flightRepository.GetByIdWithAirplaneAndSeatsAsync(returnFlightId.Value);

            var outboundSeat = outboundFlight?.Airplane.Seats.FirstOrDefault(s => s.Id == outboundSeatId.Value);
            var returnSeat = returnFlight?.Airplane.Seats.FirstOrDefault(s => s.Id == returnSeatId.Value);

            if (outboundFlight == null || returnFlight == null || outboundSeat == null || returnSeat == null)
            {
                TempData["Error"] = "Some booking data could not be loaded.";
                return RedirectToAction("Index", "Home");
            }

            var viewModel = new ConfirmRoundTripViewModel
            {
                OutboundFlight = outboundFlight,
                ReturnFlight = returnFlight,
                OutboundSeat = outboundSeat,
                ReturnSeat = returnSeat,
                IsCustomer = User.Identity.IsAuthenticated && User.IsInRole(UserRoles.Customer)

            };

            if (viewModel.IsCustomer)
            {
                var user = await _userHelper.GetUserAsync(User);
                var profile = await _customerProfileRepository.GetByUserIdAsync(user.Id);
                viewModel.ExistingPassportNumber = profile?.PassportNumber;
            }

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRoundTripCheckoutSession(string? firstName, string? lastName, string? email, string? passportNumber)
        {
            // 1. Obtenção de dados da sessão e verificação inicial
            var outboundFlightId = HttpContext.Session.GetInt32("OutboundFlightId");
            var returnFlightId = HttpContext.Session.GetInt32("ReturnFlightId");
            var outboundSeatId = HttpContext.Session.GetInt32("OutboundSeatId");
            var returnSeatId = HttpContext.Session.GetInt32("ReturnSeatId");

            if (outboundFlightId == null || returnFlightId == null || outboundSeatId == null || returnSeatId == null)
            {
                ModelState.AddModelError(string.Empty, "Booking information is incomplete.");
                return RedirectToAction("Index", "Home");
            }

            var outboundFlight = await _flightRepository.GetByIdWithAirplaneAndSeatsAsync(outboundFlightId.Value);
            var returnFlight = await _flightRepository.GetByIdWithAirplaneAndSeatsAsync(returnFlightId.Value);
            var outboundSeat = outboundFlight?.Airplane.Seats.FirstOrDefault(s => s.Id == outboundSeatId.Value);
            var returnSeat = returnFlight?.Airplane.Seats.FirstOrDefault(s => s.Id == returnSeatId.Value);

            if (outboundFlight == null || returnFlight == null || outboundSeat == null || returnSeat == null)
            {
                ModelState.AddModelError(string.Empty, "Flight or seat data is missing.");
                return RedirectToAction("Index", "Home");
            }

            // Preparar o ViewModel para, potencialmente, retornar à View com erros ou dados preenchidos
            var model = new ConfirmRoundTripViewModel
            {
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


            // 2. Lógica de validação condicional para dados pessoais (IDÊNTICA à de CreateCheckoutSession)
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
                else
                {
                    if (string.IsNullOrWhiteSpace(passportNumber))
                    {
                        passportNumber = profile.PassportNumber;
                        model.PassportNumber = profile.PassportNumber; // Atualiza o modelo para a view
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
                        ModelState.AddModelError("Email", "An account with this email already exists.");
                }

                if (string.IsNullOrWhiteSpace(passportNumber))
                    ModelState.AddModelError("PassportNumber", "Passport number is required.");
                else
                {
                    var existingProfile = await _customerProfileRepository.GetByPassportAsync(passportNumber);
                    if (existingProfile != null)
                        ModelState.AddModelError("PassportNumber", "This passport number is already in use.");
                }

                if (ModelState.IsValid)
                {
                    HttpContext.Session.SetString("FirstName", firstName!);
                    HttpContext.Session.SetString("LastName", lastName!);
                    HttpContext.Session.SetString("Email", email!);
                    HttpContext.Session.SetString("PassportNumber", passportNumber!);
                }
            }

            // 3. VERIFICAÇÃO FINAL DO MODELSTATE
            if (!ModelState.IsValid)
            {
                return View("ConfirmRoundTrip", model);
            }

            // 4. Se tudo válido, procede com a criação da sessão de pagamento
            var priceOutbound = model.OutboundPrice;
            var priceReturn = model.ReturnPrice;
            var totalPrice = model.TotalPrice;

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

            decimal priceOutbound = decimal.Parse(outboundPriceStr, CultureInfo.InvariantCulture);
            decimal priceReturn = decimal.Parse(returnPriceStr, CultureInfo.InvariantCulture);
            string userId;

            if (User.Identity.IsAuthenticated && User.IsInRole(UserRoles.Customer))
            {
                var email = User.Identity.Name;
                var user = await _userHelper.GetUserByEmailAsync(email);
                if (user == null)
                {
                    TempData["Error"] = "Could not retrieve user data.";
                    return RedirectToAction("Index", "Home");
                }

                userId = user.Id;
            }
            else
            {
                var email = HttpContext.Session.GetString("Email");
                var firstName = HttpContext.Session.GetString("FirstName");
                var lastName = HttpContext.Session.GetString("LastName");
                var passportNumber = HttpContext.Session.GetString("PassportNumber");

                if (string.IsNullOrWhiteSpace(email))
                {
                    TempData["Error"] = "Missing user email.";
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

                var token = await _userHelper.GenerateEmailConfirmationTokenAsync(user);
                await _userHelper.ConfirmEmailAsync(user, token);
                await _userHelper.CheckRoleAsync(UserRoles.Customer);
                await _userHelper.AddUserToRoleAsync(user, UserRoles.Customer);

                var profile = new CustomerProfile
                {
                    UserId = user.Id,
                    PassportNumber = passportNumber
                };
                await _customerProfileRepository.CreateAsync(profile);

                var resetToken = await _userHelper.GeneratePasswordResetTokenAsync(user);
                var resetLink = Url.Action("ResetPassword", "Account", new { email = user.Email, token = resetToken }, Request.Scheme);
                var body = $"<p>Hello {user.FullName},</p>" +
                           $"<p>Thank you for booking with Vitoria Airlines.</p>" +
                           $"<p>You have been registered as a customer. Click below to set your password:</p>" +
                           $"<p><a href='{resetLink}'>Set Password</a></p>";

                await _mailHelper.SendEmailAsync(user.Email, "Set your Vitoria Airlines password", body);

                userId = user.Id;
            }

            var ticket1Exists = await _ticketRepository.UserHasTicketForFlightAsync(userId, outboundFlightId.Value);
            var ticket2Exists = await _ticketRepository.UserHasTicketForFlightAsync(userId, returnFlightId.Value);

            if (ticket1Exists || ticket2Exists)
            {
                TempData["Error"] = "You already have a ticket for one of the selected flights.";
                return RedirectToAction("Index", "Home");
            }

            var ticketOutbound = new Ticket
            {
                FlightId = outboundFlightId.Value,
                SeatId = outboundSeatId.Value,
                UserId = userId,
                PricePaid = priceOutbound,
                PurchaseDateUtc = DateTime.UtcNow
            };

            var ticketReturn = new Ticket
            {
                FlightId = returnFlightId.Value,
                SeatId = returnSeatId.Value,
                UserId = userId,
                PricePaid = priceReturn,
                PurchaseDateUtc = DateTime.UtcNow
            };

            await _ticketRepository.CreateAsync(ticketOutbound);
            await _ticketRepository.CreateAsync(ticketReturn);

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




    }
}
