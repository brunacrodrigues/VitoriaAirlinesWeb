using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using VitoriaAirlinesWeb.Data.Entities;
using VitoriaAirlinesWeb.Data.Enums;
using VitoriaAirlinesWeb.Models.Airports;
using VitoriaAirlinesWeb.Models.ViewModels.Airplanes;
using VitoriaAirlinesWeb.Models.ViewModels.Booking;
using VitoriaAirlinesWeb.Models.ViewModels.Customers;
using VitoriaAirlinesWeb.Models.ViewModels.Dashboard;
using VitoriaAirlinesWeb.Models.ViewModels.Employees;
using VitoriaAirlinesWeb.Models.ViewModels.Flights;
using VitoriaAirlinesWeb.Models.ViewModels.Tickets;

namespace VitoriaAirlinesWeb.Helpers
{
    /// <summary>
    /// Provides helper methods for converting between data entities and view models,
    /// and for updating entity properties from view model data.
    /// </summary>
    public class ConverterHelper : IConverterHelper
    {
        /// <summary>
        /// Updates a CustomerProfile entity with data from a CustomerProfileViewModel.
        /// </summary>
        /// <param name="entity">The CustomerProfile entity to update.</param>
        /// <param name="model">The CustomerProfileViewModel containing the new data.</param>
        public void UpdateCustomerProfile(CustomerProfile entity, CustomerProfileViewModel model)
        {
            entity.CountryId = model.CountryId;
            entity.PassportNumber = model.PassportNumber;
        }


        /// <summary>
        /// Updates a CustomerProfile entity with data from a CustomerProfileAdminViewModel.
        /// </summary>
        /// <param name="entity">The CustomerProfile entity to update.</param>
        /// <param name="model">The CustomerProfileAdminViewModel containing the new data.</param>
        public void UpdateCustomerProfile(CustomerProfile entity, CustomerProfileAdminViewModel model)
        {
            entity.CountryId = model.CountryId;
            entity.PassportNumber = model.PassportNumber;
        }


        /// <summary>
        /// Converts a CustomerProfile entity to a CustomerProfileAdminViewModel.
        /// </summary>
        /// <param name="entity">The CustomerProfile entity to convert.</param>
        /// <returns>A new CustomerProfileAdminViewModel.</returns>
        public CustomerProfileAdminViewModel ToCustomerProfileAdminViewModel(CustomerProfile entity)
        {
            return new CustomerProfileAdminViewModel
            {
                Id = entity.Id,
                CountryId = entity.CountryId,
                PassportNumber = entity.PassportNumber,
                FullName = entity.User.FullName,
                Email = entity.User.Email
            };
        }


        /// <summary>
        /// Converts a CustomerProfile entity and a User entity to a CustomerProfileViewModel.
        /// </summary>
        /// <param name="profile">The CustomerProfile entity.</param>
        /// <param name="user">The User entity.</param>
        /// <returns>A new CustomerProfileViewModel.</returns>
        public CustomerProfileViewModel ToCustomerProfileViewModel(CustomerProfile profile, User user)
        {
            return new CustomerProfileViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                CurrentProfileImagePath = user.ImageFullPath,
                CountryId = profile.CountryId,
                PassportNumber = profile.PassportNumber,
            };
        }


        /// <summary>
        /// Converts an AirplaneViewModel to an Airplane entity.
        /// </summary>
        /// <param name="model">The AirplaneViewModel to convert.</param>
        /// <param name="imageId">The GUID of the airplane's image.</param>
        /// <param name="isNew">A flag indicating if this is a new airplane (true) or an existing one (false).</param>
        /// <returns>A new Airplane entity.</returns>
        public Airplane ToAirplane(AirplaneViewModel model, Guid imageId, bool isNew)
        {
            return new Airplane
            {
                Id = isNew ? 0 : model.Id,
                Model = model.Model,
                TotalExecutiveSeats = model.TotalExecutiveSeats,
                TotalEconomySeats = model.TotalEconomySeats,
                ImageId = imageId,
                Status = isNew ? AirplaneStatus.Active : model.Status
            };
        }


        /// <summary>
        /// Converts an Airplane entity to an AirplaneViewModel.
        /// </summary>
        /// <param name="entity">The Airplane entity to convert.</param>
        /// <returns>A new AirplaneViewModel.</returns>
        public AirplaneViewModel ToAirplaneViewModel(Airplane entity)
        {
            return new AirplaneViewModel
            {
                Id = entity.Id,
                Model = entity.Model,
                TotalExecutiveSeats = entity.TotalExecutiveSeats,
                TotalEconomySeats = entity.TotalEconomySeats,
                ImageId = entity.ImageId,
                Status = entity.Status
            };
        }


        /// <summary>
        /// Converts an AirportViewModel to an Airport entity.
        /// </summary>
        /// <param name="model">The AirportViewModel to convert.</param>
        /// <param name="isNew">A flag indicating if this is a new airport (true) or an existing one (false).</param>
        /// <returns>A new Airport entity.</returns>
        public Airport ToAirport(AirportViewModel model, bool isNew)
        {
            return new Airport
            {
                Id = isNew ? 0 : model.Id,
                IATA = model.IATA.ToUpper(),
                Name = model.Name,
                City = model.City,
                CountryId = model.CountryId.Value
            };
        }


        /// <summary>
        /// Converts an Airport entity to an AirportViewModel.
        /// </summary>
        /// <param name="entity">The Airport entity to convert.</param>
        /// <returns>A new AirportViewModel.</returns>
        public AirportViewModel ToAirportViewModel(Airport entity)
        {
            return new AirportViewModel
            {
                Id = entity.Id,
                IATA = entity.IATA,
                Name = entity.Name,
                City = entity.City,
                CountryId = entity.CountryId
            };
        }


        /// <summary>
        /// Converts a FlightViewModel to a Flight entity.
        /// </summary>
        /// <param name="model">The FlightViewModel to convert.</param>
        /// <param name="isNew">A flag indicating if this is a new flight (true) or an existing one (false).</param>
        /// <returns>A new Flight entity.</returns>
        public Flight ToFlight(FlightViewModel model, bool isNew)
        {
            return new Flight
            {
                Id = isNew ? 0 : model.Id,
                FlightNumber = model.FlightNumber!,
                OriginAirportId = model.OriginAirportId.Value,
                DestinationAirportId = model.DestinationAirportId.Value,
                AirplaneId = model.AirplaneId,
                EconomyClassPrice = model.EconomyClassPrice,
                ExecutiveClassPrice = model.ExecutiveClassPrice,
                DepartureUtc = TimezoneHelper.ConvertToUtc(DateTime.SpecifyKind(
                    model.DepartureDate.Value.ToDateTime(model.DepartureTime.Value), DateTimeKind.Unspecified)),
                Duration = model.Duration.Value,
                Status = isNew ? FlightStatus.Scheduled : model.Status
            };
        }


        /// <summary>
        /// Converts a Flight entity to a FlightViewModel.
        /// </summary>
        /// <param name="entity">The Flight entity to convert.</param>
        /// <returns>A new FlightViewModel.</returns>
        public FlightViewModel ToFlightViewModel(Flight entity)
        {
            var localDeparture = TimezoneHelper.ConvertToLocal(entity.DepartureUtc);

            return new FlightViewModel
            {
                Id = entity.Id,
                FlightNumber = entity.FlightNumber,
                OriginAirportId = entity.OriginAirportId,
                DestinationAirportId = entity.DestinationAirportId,
                AirplaneId = entity.AirplaneId,
                EconomyClassPrice = entity.EconomyClassPrice,
                ExecutiveClassPrice = entity.ExecutiveClassPrice,
                DepartureDate = DateOnly.FromDateTime(localDeparture),
                DepartureTime = TimeOnly.FromDateTime(localDeparture),
                Duration = entity.Duration,
                Status = entity.Status
            };
        }


        /// <summary>
        /// Updates a User entity with data from an EditEmployeeViewModel.
        /// </summary>
        /// <param name="model">The EditEmployeeViewModel containing the new data.</param>
        /// <param name="user">The User entity to update.</param>
        /// <returns>The updated User entity.</returns>
        public User ToUser(EditEmployeeViewModel model, User user)
        {
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            return user;
        }



        /// <summary>
        /// Converts a User entity to an EditEmployeeViewModel.
        /// </summary>
        /// <param name="entity">The User entity to convert.</param>
        /// <returns>A new EditEmployeeViewModel.</returns>
        public EditEmployeeViewModel ToEditEmployeeViewModel(User entity)
        {
            return new EditEmployeeViewModel
            {
                Email = entity.Email,
                FirstName = entity.FirstName,
                LastName = entity.LastName
            };
        }


        /// <summary>
        /// Converts a Flight entity and a list of occupied seat IDs to a SelectSeatViewModel.
        /// </summary>
        /// <param name="entity">The Flight entity.</param>
        /// <param name="occupiedSeatsIds">A collection of IDs for occupied seats on the flight.</param>
        /// <returns>A new SelectSeatViewModel.</returns>
        public SelectSeatViewModel ToSelectSeatViewModel(Flight entity, IEnumerable<int> occupiedSeatsIds)
        {
            return new SelectSeatViewModel
            {
                FlightId = entity.Id,
                FlightInfo = entity.FlightInfo,
                OriginAirport = entity.OriginAirport,
                DestinationAirport = entity.DestinationAirport,
                EconomyPrice = entity.EconomyClassPrice,
                ExecutivePrice = entity.ExecutiveClassPrice,
                Seats = entity.Airplane.Seats.ToList(),
                OccupiedSeatsIds = occupiedSeatsIds.ToHashSet()
            };
        }


        /// <summary>
        /// Converts a Flight entity and a Seat entity to a ConfirmBookingViewModel.
        /// </summary>
        /// <param name="flight">The Flight entity.</param>
        /// <param name="seat">The Seat entity.</param>
        /// <returns>A new ConfirmBookingViewModel.</returns>
        public ConfirmBookingViewModel ToConfirmBookingViewModel(Flight flight, Seat seat)
        {
            return new ConfirmBookingViewModel
            {
                FlightId = flight.Id,
                SeatId = seat.Id,
                FlightNumber = flight.FlightNumber,
                DepartureInfo = $"{flight.OriginAirport.Name} ({flight.OriginAirport.IATA})",
                ArrivalInfo = $"{flight.DestinationAirport.Name} ({flight.DestinationAirport.IATA})",
                DepartureTime = TimezoneHelper.ConvertToLocal(flight.DepartureUtc),
                ArrivalTime = TimezoneHelper.ConvertToLocal(flight.ArrivalUtc),
                SeatInfo = $"Row {seat.Row}, Seat {seat.Letter}",
                SeatClass = seat.Class.ToString(),
                FinalPrice = seat.Class == SeatClass.Executive ? flight.ExecutiveClassPrice : flight.EconomyClassPrice
            };
        }



        /// <summary>
        /// Converts old ticket details and a new seat to a ConfirmSeatChangeViewModel.
        /// </summary>
        /// <param name="oldTicket">The original Ticket entity.</param>
        /// <param name="newSeat">The newly selected Seat entity.</param>
        /// <param name="newPrice">The price of the new seat.</param>
        /// <returns>A new ConfirmSeatChangeViewModel.</returns>
        public ConfirmSeatChangeViewModel ToConfirmSeatChangeViewModel(Ticket oldTicket, Seat newSeat, decimal newPrice)
        {
            var flight = oldTicket.Flight;
            var oldPrice = oldTicket.PricePaid;
            var priceDifference = newPrice - oldPrice;

            return new ConfirmSeatChangeViewModel
            {
                OldTicketId = oldTicket.Id,
                NewSeatId = newSeat.Id,
                FlightNumber = flight.FlightNumber,
                DepartureInfo = $"{flight.OriginAirport.City} ({flight.OriginAirport.IATA})",
                ArrivalInfo = $"{flight.DestinationAirport.City} ({flight.DestinationAirport.IATA})",
                DepartureTime = TimezoneHelper.ConvertToLocal(flight.DepartureUtc),
                ArrivalTime = TimezoneHelper.ConvertToLocal(flight.ArrivalUtc),
                OldSeatInfo = $"{oldTicket.Seat.Row}{oldTicket.Seat.Letter}",
                OldSeatClass = oldTicket.Seat.Class.ToString(),
                NewSeatInfo = $"{newSeat.Row}{newSeat.Letter}",
                NewSeatClass = newSeat.Class.ToString(),
                OldPricePaid = oldPrice,
                NewPrice = newPrice,
                PriceDifference = priceDifference
            };
        }


        /// <summary>
        /// Converts a Flight entity to a FlightDashboardViewModel.
        /// </summary>
        /// <param name="entity">The Flight entity to convert.</param>
        /// <returns>A new FlightDashboardViewModel.</returns>
        public FlightDashboardViewModel ToFlightDashboardViewModel(Flight entity)
        {
            return new FlightDashboardViewModel
            {
                Id = entity.Id,
                FlightNumber = entity.FlightNumber,

                AirplaneModel = entity.Airplane.Model,

                OriginAirportFullName = entity.OriginAirport.FullName,
                OriginCountryFlagUrl = entity.OriginAirport.Country?.FlagImageUrl ?? "",

                DestinationAirportFullName = entity.DestinationAirport.FullName,
                DestinationCountryFlagUrl = entity.DestinationAirport.Country?.FlagImageUrl ?? "",

                DepartureFormatted = TimezoneHelper.ConvertToLocal(entity.DepartureUtc).ToString("HH:mm dd MMM"),
                DepartureIso = TimezoneHelper.ConvertToLocal(entity.DepartureUtc).ToString("o"),

            };
        }


        /// <summary>
        /// Updates a Flight entity with data from a FlightViewModel.
        /// </summary>
        /// <param name="entity">The Flight entity to update.</param>
        /// <param name="model">The FlightViewModel containing the new data.</param>
        public void UpdateFlightFromViewModel(Flight entity, FlightViewModel model)
        {
            entity.OriginAirportId = model.OriginAirportId!.Value;
            entity.DestinationAirportId = model.DestinationAirportId!.Value;
            entity.AirplaneId = model.AirplaneId;
            entity.EconomyClassPrice = model.EconomyClassPrice;
            entity.ExecutiveClassPrice = model.ExecutiveClassPrice;
            entity.DepartureUtc = TimezoneHelper.ConvertToUtc(
            DateTime.SpecifyKind(
                model.DepartureDate.Value.ToDateTime(model.DepartureTime.Value), DateTimeKind.Unspecified));
            entity.Duration = model.Duration!.Value;
        }


        /// <summary>
        /// Converts a collection of Flight entities to a list of FlightDisplayViewModel.
        /// </summary>
        /// <param name="flights">The collection of Flight entities to convert.</param>
        /// <returns>A list of new FlightDisplayViewModel objects.</returns>
        public List<FlightDisplayViewModel> ToFlightDisplayViewModel(IEnumerable<Flight> flights)
        {

            return flights.Select(f => new FlightDisplayViewModel
            {
                Id = f.Id,
                FlightNumber = f.FlightNumber,
                AirplaneModel = f.Airplane.Model,
                Origin = f.OriginAirport.FullName,
                OriginFlagUrl = f.OriginAirport.Country?.FlagImageUrl ?? "",
                Destination = f.DestinationAirport.FullName,
                DestinationFlagUrl = f.DestinationAirport.Country?.FlagImageUrl ?? "",
                Departure = TimezoneHelper.ConvertToLocal(f.DepartureUtc).ToString("yyyy-MM-dd HH:mm"),
                Arrival = TimezoneHelper.ConvertToLocal(f.ArrivalUtc).ToString("yyyy-MM-dd HH:mm"),
                Duration = f.Duration.ToString(@"hh\:mm"),
                ExecutiveClassPrice = f.ExecutiveClassPrice,
                EconomyClassPrice = f.EconomyClassPrice,
                Status = f.Status.ToString()
            }).ToList();
        }


        /// <summary>
        /// Converts a collection of Ticket entities to a list of TicketDisplayViewModel.
        /// </summary>
        /// <param name="tickets">The collection of Ticket entities to convert.</param>
        /// <returns>A list of new TicketDisplayViewModel objects.</returns>
        public List<TicketDisplayViewModel> ToTicketDisplayViewModel(IEnumerable<Ticket> tickets)
        {

            return tickets.Select(t => new TicketDisplayViewModel
            {
                Id = t.Id,
                FlightNumber = t.Flight.FlightNumber,
                Origin = t.Flight.OriginAirport.FullName,
                OriginFlagUrl = t.Flight.OriginAirport.Country?.FlagImageUrl ?? "",
                Destination = t.Flight.DestinationAirport.FullName,
                DestinationFlagUrl = t.Flight.DestinationAirport.Country?.FlagImageUrl ?? "",
                Departure = TimezoneHelper.ConvertToLocal(t.Flight.DepartureUtc),
                Arrival = TimezoneHelper.ConvertToLocal(t.Flight.ArrivalUtc),
                SeatDisplay = $"{t.Seat.Row}{t.Seat.Letter}",
                Class = t.Seat.Class.ToString(),
                PricePaid = t.PricePaid,
                Status = t.Flight.Status.ToString()

            }).ToList();
        }
    }
}
