using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VitoriaAirlinesAPI.Dtos;
using VitoriaAirlinesWeb.Data.Enums;
using VitoriaAirlinesWeb.Data.Repositories;
using VitoriaAirlinesWeb.Helpers;
using VitoriaAirlinesWeb.Models.Dtos;

namespace VitoriaAirlinesAPI.Controllers
{
    /// <summary>
    /// API controller responsible for retrieving flight-related information for users.
    /// Requires JWT authentication by default, except for explicitly allowed actions.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]

    public class FlightsController : ControllerBase
    {
        private readonly ITicketRepository _ticketRepository;
        private readonly IUserHelper _userHelper;
        private readonly IFlightRepository _flightRepository;


        /// <summary>
        /// Initializes a new instance of the <see cref="FlightsController"/> class.
        /// </summary>
        /// <param name="ticketRepository">Repository used for accessing ticket data.</param>
        /// <param name="userHelper">Helper service used for user-related operations.</param>
        public FlightsController(ITicketRepository ticketRepository, IUserHelper userHelper, IFlightRepository flightRepository)
        {
            _ticketRepository = ticketRepository;
            _userHelper = userHelper;
            _flightRepository = flightRepository;
        }


        /// <summary>
        /// Retrieves the list of upcoming flights for a given user based on their user ID.
        /// </summary>
        /// <param name="userId">The ID of the user whose upcoming flights are to be retrieved.</param>
        /// <returns>A list of upcoming flight tickets in a simplified DTO format, or an error message.</returns>
        [HttpGet("by-id/{userId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetUserUpcomingFlightsById(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return BadRequest("UserId is required.");

            var user = await _userHelper.GetUserByIdAsync(userId);
            if (user == null)
                return NotFound("User not found.");

            var tickets = await _ticketRepository.GetUpcomingTicketsByUserAsync(user.Id);

            var result = tickets.Select(t => new TicketDto
            {
                TicketId = t.Id,
                FlightNumber = t.Flight.FlightNumber,
                DepartureUtc = t.Flight.DepartureUtc,
                OriginAirport = t.Flight.OriginAirport.IATA,
                DestinationAirport = t.Flight.DestinationAirport.IATA,
                Seat = $"{t.Seat.Row}{t.Seat.Letter} {t.Seat.Class}",
                PricePaid = t.PricePaid,
                PurchaseDateUtc = t.PurchaseDateUtc
            });

            return Ok(result);
        }


        /// <summary>
        /// Retrieves the list of upcoming flights for a given user based on their email.
        /// </summary>
        /// <param name="email">The email of the user whose upcoming flights are to be retrieved.</param>
        /// <returns>A list of upcoming flight tickets in a simplified DTO format, or an error message.</returns>
        [HttpGet("by-email/{email}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetUserUpcomingFlightsByEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return BadRequest("Email is required.");

            var user = await _userHelper.GetUserByEmailAsync(email);
            if (user == null)
                return NotFound("User not found.");

            var tickets = await _ticketRepository.GetUpcomingTicketsByUserAsync(user.Id);

            var result = tickets.Select(t => new TicketDto
            {
                TicketId = t.Id,
                FlightNumber = t.Flight.FlightNumber,
                DepartureUtc = t.Flight.DepartureUtc,
                OriginAirport = t.Flight.OriginAirport.IATA,
                DestinationAirport = t.Flight.DestinationAirport.IATA,
                Seat = $"{t.Seat.Row}{t.Seat.Letter} {t.Seat.Class}",
                PricePaid = t.PricePaid,
                PurchaseDateUtc = t.PurchaseDateUtc
            });

            return Ok(result);
        }



        [HttpGet("upcoming/me")]
        public async Task<IActionResult> GetMyUpcomingFlights()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var tickets = await _ticketRepository.GetUpcomingTicketsByUserAsync(userId);

            var result = tickets.Select(t => new TicketDto
            {
                TicketId = t.Id,
                FlightId = t.FlightId,
                FlightNumber = t.Flight.FlightNumber,
                DepartureUtc = t.Flight.DepartureUtc,
                ArrivalUtc = t.Flight.ArrivalUtc,
                OriginAirport = t.Flight.OriginAirport.FullName,
                DestinationAirport = t.Flight.DestinationAirport.FullName,
                OriginCountryCode = t.Flight.OriginAirport.Country.CountryCode,
                DestinationCountryCode = t.Flight.DestinationAirport.Country.CountryCode,
                Seat = $"{t.Seat.Row}{t.Seat.Letter} ({t.Seat.Class})",
                PricePaid = t.PricePaid,
                PurchaseDateUtc = t.PurchaseDateUtc,
                Status = t.Flight.Status.ToString()

            }).ToList();

            return Ok(result);
        }


        [HttpGet("history/me")]
        public async Task<IActionResult> GetMyFlightHistory()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var tickets = await _ticketRepository.GetCompletedFlightsByUserAsync(userId);
            var result = tickets.Select(t => new TicketDto
            {
                TicketId = t.Id,
                FlightId = t.FlightId,
                FlightNumber = t.Flight.FlightNumber,
                DepartureUtc = t.Flight.DepartureUtc,
                ArrivalUtc = t.Flight.ArrivalUtc,
                OriginAirport = t.Flight.OriginAirport.FullName,
                DestinationAirport = t.Flight.DestinationAirport.FullName,
                OriginCountryCode = t.Flight.OriginAirport.Country.CountryCode,
                DestinationCountryCode = t.Flight.DestinationAirport.Country.CountryCode,
                Seat = $"{t.Seat.Row}{t.Seat.Letter} ({t.Seat.Class})",
                PricePaid = t.PricePaid,
                PurchaseDateUtc = t.PurchaseDateUtc,
                Status = t.Flight.Status.ToString(),

            }).ToList();
            return Ok(result);
        }


        [HttpPost("search")]
        [AllowAnonymous] // Flight search can be anonymous
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SearchFlights([FromBody] FlightSearchRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Additional date validations
            if (request.DepartureDate.Date < DateTime.UtcNow.Date)
            {
                return BadRequest("Departure date cannot be in the past.");
            }

            if (request.IsRoundTrip && (!request.ReturnDate.HasValue || request.ReturnDate.Value.Date < request.DepartureDate.Date))
            {
                return BadRequest("Return date is required for round trips and cannot be earlier than the departure date.");
            }

            List<int> alreadyBookedFlightIds = new List<int>();
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!string.IsNullOrEmpty(userId))
            {
                alreadyBookedFlightIds = (await _ticketRepository.GetFlightIdsForUserAsync(userId)).ToList();
            }

            // Search for outbound flights
            var outboundFlightsEntities = await _flightRepository.GetScheduledFlightsByCriteriaAsync(
                request.OriginAirportId,
                request.DestinationAirportId,
                request.DepartureDate,
                request.NumberOfPassengers);

            var outboundFlightResults = new List<FlightSearchResultDto>();
            foreach (var flight in outboundFlightsEntities)
            {
                if (!string.IsNullOrEmpty(userId) && alreadyBookedFlightIds.Contains(flight.Id))
                {
                    continue;
                }
                var totalCapacity = flight.Airplane.TotalEconomySeats + flight.Airplane.TotalExecutiveSeats;
                var soldTickets = flight.Tickets?.Count ?? 0;
                var availableSeats = totalCapacity - soldTickets;

                outboundFlightResults.Add(new FlightSearchResultDto
                {
                    FlightId = flight.Id,
                    FlightNumber = flight.FlightNumber,
                    OriginAirportIATA = flight.OriginAirport.IATA,
                    OriginAirportFullName = flight.OriginAirport.FullName,
                    OriginCountryCode = flight.OriginAirport.Country.CountryCode,
                    DestinationAirportIATA = flight.DestinationAirport.IATA,
                    DestinationAirportFullName = flight.DestinationAirport.FullName,
                    DestinationCountryCode = flight.DestinationAirport.Country.CountryCode,
                    DepartureUtc = flight.DepartureUtc,
                    ArrivalUtc = flight.ArrivalUtc,
                    Duration = flight.Duration,
                    EconomyClassPrice = flight.EconomyClassPrice,
                    ExecutiveClassPrice = flight.ExecutiveClassPrice,
                    AvailableSeats = availableSeats,
                    Status = flight.Status.ToString()
                });
            }

            // If it's a round trip, also search for return flights
            if (request.IsRoundTrip && request.ReturnDate.HasValue)
            {
                var returnFlightsEntities = await _flightRepository.GetScheduledFlightsByCriteriaAsync(
                    request.DestinationAirportId, // Origin for the return leg
                    request.OriginAirportId,      // Destination for the return leg
                    request.ReturnDate.Value,
                    request.NumberOfPassengers);

                var returnFlightResults = new List<FlightSearchResultDto>();
                foreach (var flight in returnFlightsEntities)
                {
                    if (!string.IsNullOrEmpty(userId) && alreadyBookedFlightIds.Contains(flight.Id))
                    {
                        continue;
                    }

                    var totalCapacity = flight.Airplane.TotalEconomySeats + flight.Airplane.TotalExecutiveSeats;
                    var soldTickets = flight.Tickets?.Count ?? 0;
                    var availableSeats = totalCapacity - soldTickets;

                    returnFlightResults.Add(new FlightSearchResultDto
                    {
                        FlightId = flight.Id,
                        FlightNumber = flight.FlightNumber,
                        OriginAirportIATA = flight.OriginAirport.IATA,
                        OriginAirportFullName = flight.OriginAirport.FullName,
                        OriginCountryCode = flight.OriginAirport.Country.CountryCode,
                        DestinationAirportIATA = flight.DestinationAirport.IATA,
                        DestinationAirportFullName = flight.DestinationAirport.FullName,
                        DestinationCountryCode = flight.DestinationAirport.Country.CountryCode,
                        DepartureUtc = flight.DepartureUtc,
                        ArrivalUtc = flight.ArrivalUtc,
                        Duration = flight.Duration,
                        EconomyClassPrice = flight.EconomyClassPrice,
                        ExecutiveClassPrice = flight.ExecutiveClassPrice,
                        AvailableSeats = availableSeats,
                        Status = flight.Status.ToString()
                    });
                }


                if (!outboundFlightResults.Any() || !returnFlightResults.Any())
                {
                    // Retorna listas vazias para indicar que não há voos completos de ida e volta
                    // Isso fará com que o MAUI exiba a mensagem de "no flights found"
                    return Ok(new { OutboundFlights = new List<FlightSearchResultDto>(), ReturnFlights = new List<FlightSearchResultDto>() });
                }

                // Se ambos tiverem voos, retorna os resultados de ida e volta
                return Ok(new { OutboundFlights = outboundFlightResults, ReturnFlights = returnFlightResults });
            }

            // Return only outbound flights for one-way trips
            // Esta parte permanece inalterada, pois é para viagens de apenas ida.
            return Ok(new { OutboundFlights = outboundFlightResults });
        }


        [HttpGet("{flightId}/seats")]
        [AllowAnonymous] // Permitir acesso a anónimos para a seleção de lugares, como na aplicação web
        [ProducesResponseType(typeof(FlightSeatsResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)] // Voo não encontrado
        [ProducesResponseType(StatusCodes.Status400BadRequest)] // Voo indisponível ou utilizador já reservou
        public async Task<IActionResult> GetFlightSeats(int flightId)
        {
            // 1. Obter os detalhes completos do voo, incluindo avião, assentos e tickets associados.
            var flight = await _flightRepository.GetByIdWithAirplaneAndSeatsAsync(flightId);

            if (flight == null)
            {
                return NotFound($"Flight with ID {flightId} not found.");
            }

            // 2. Verificar o estado do voo (deve estar 'Scheduled' para permitir reservas).
            if (flight.Status != FlightStatus.Scheduled)
            {
                return BadRequest($"Flight {flight.FlightNumber} is no longer available for booking (Status: {flight.Status}).");
            }

            // 3. Verificação: O utilizador autenticado já tem um bilhete para este voo?
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Obtém o ID do utilizador logado.

            if (!string.IsNullOrEmpty(userId)) // Se o utilizador está autenticado
            {
                // Usar o novo método otimizado para verificar se já tem bilhete para este voo.
                var alreadyBooked = await _ticketRepository.UserHasTicketForFlightAsync(userId, flightId);
                if (alreadyBooked)
                {
                    // Retornar um erro claro. O MAUI terá de capturar e exibir esta mensagem.
                    return BadRequest("You have already booked a ticket for this flight.");
                }
            }

            var allTickets = await _ticketRepository.GetTicketsByFlightAsync(flightId);
            // 4. Obter os IDs dos lugares que já estão ocupados (com bilhetes não cancelados).
            // `flight.Tickets` já deve conter os tickets graças ao `.Include()` em `GetByIdWithAirplaneAndSeatsAsync`.
            var occupiedSeatIds = allTickets.Where(t => !t.IsCanceled).Select(t => t.SeatId).ToList();

            // 5. Mapear os assentos do avião para os DTOs de detalhe de assento, marcando os ocupados.
            var seatDetails = flight.Airplane.Seats
                                    .OrderBy(s => s.Row)
                                    .ThenBy(s => s.Letter)
                                    .Select(s => new SeatDetailDto
                                    {
                                        Id = s.Id,
                                        Row = s.Row,
                                        Letter = s.Letter,
                                        Class = s.Class,
                                        IsOccupied = occupiedSeatIds.Contains(s.Id)
                                    })
                                    .ToList();

            // 6. Criar e retornar o DTO de resposta completo.
            var response = new FlightSeatsResponseDto
            {
                FlightId = flight.Id,
                FlightNumber = flight.FlightNumber,
                OriginAirportIATA = flight.OriginAirport.IATA,
                DestinationAirportIATA = flight.DestinationAirport.IATA,
                OriginAirportName = flight.OriginAirport.Name,
                DestinationAirportName = flight.DestinationAirport.Name,
                OriginCountryCode = flight.OriginAirport.Country.CountryCode,
                DestinationCountryCode = flight.DestinationAirport.Country.CountryCode,
                DepartureUtc = flight.DepartureUtc,
                ArrivalUtc = flight.ArrivalUtc,
                Duration = flight.Duration,
                EconomyClassPrice = flight.EconomyClassPrice,
                ExecutiveClassPrice = flight.ExecutiveClassPrice,
                Seats = seatDetails
            };

            return Ok(response);
        }
    }


}