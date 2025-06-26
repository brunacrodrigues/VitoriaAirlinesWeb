using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using VitoriaAirlinesWeb.Data.Entities;
using VitoriaAirlinesWeb.Data.Enums;
using VitoriaAirlinesWeb.Models.Airports;
using VitoriaAirlinesWeb.Models.ViewModels.Airplanes;
using VitoriaAirlinesWeb.Models.ViewModels.Booking;
using VitoriaAirlinesWeb.Models.ViewModels.Customers;
using VitoriaAirlinesWeb.Models.ViewModels.Employees;
using VitoriaAirlinesWeb.Models.ViewModels.Flights;

namespace VitoriaAirlinesWeb.Helpers
{
    public class ConverterHelper : IConverterHelper
    {
        public void UpdateCustomerProfile(CustomerProfile entity, CustomerProfileViewModel model)
        {
            entity.CountryId = model.CountryId;
            entity.PassportNumber = model.PassportNumber;
        }

        public void UpdateCustomerProfile(CustomerProfile entity, CustomerProfileAdminViewModel model)
        {
            entity.CountryId = model.CountryId;
            entity.PassportNumber = model.PassportNumber;
        }

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

        public CustomerProfileViewModel ToCustomerProfileViewModel(CustomerProfile entity)
        {
            return new CustomerProfileViewModel
            {
                CountryId = entity.CountryId,
                PassportNumber = entity.PassportNumber
            };
        }

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

        public Airport ToAirport(AirportViewModel model, bool isNew)
        {
            return new Airport
            {
                Id = isNew ? 0 : model.Id,
                IATA = model.IATA.ToUpper(),
                Name = model.Name,
                City = model.City,
                CountryId = model.CountryId
            };
        }

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
                DepartureUtc = model.DepartureDate.Value.ToDateTime(model.DepartureTime.Value).ToUniversalTime(),
                Duration = model.Duration.Value,
                Status = isNew ? FlightStatus.Scheduled : model.Status
            };
        }

        public FlightViewModel ToFlightViewModel(Flight entity)
        {
            return new FlightViewModel
            {
                Id = entity.Id,
                FlightNumber = entity.FlightNumber,
                OriginAirportId = entity.OriginAirportId,
                DestinationAirportId = entity.DestinationAirportId,
                AirplaneId = entity.AirplaneId,
                EconomyClassPrice = entity.EconomyClassPrice,
                ExecutiveClassPrice = entity.ExecutiveClassPrice,
                DepartureDate = DateOnly.FromDateTime(entity.DepartureUtc.ToLocalTime()),
                DepartureTime = TimeOnly.FromDateTime(entity.DepartureUtc.ToLocalTime()),
                Duration = entity.Duration,
                Status = entity.Status
            };
        }

        public User ToUser(EditEmployeeViewModel model, User user)
        {
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            return user;
        }

        public EditEmployeeViewModel ToEditEmployeeViewModel(User entity)
        {
            return new EditEmployeeViewModel
            {
                Email = entity.Email,
                FirstName = entity.FirstName,
                LastName = entity.LastName
            };
        }

        public SelectSeatViewModel ToSelectSeatViewModelAsync(Flight entity, IEnumerable<int> occupiedSeatsIds)
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

        public ConfirmBookingViewModel ToConfirmBookingViewModel(Flight flight, Seat seat)
        {
            return new ConfirmBookingViewModel
            {
                FlightId = flight.Id,
                SeatId = seat.Id,
                FlightNumber = flight.FlightNumber,
                DepartureInfo = $"{flight.OriginAirport.Name} ({flight.OriginAirport.IATA})",
                ArrivalInfo = $"{flight.DestinationAirport.Name} ({flight.DestinationAirport.IATA})",
                DepartureTime = flight.DepartureUtc.ToLocalTime(),
                ArrivalTime = flight.ArrivalUtc.ToLocalTime(),
                SeatInfo = $"Row {seat.Row}, Seat {seat.Letter}",
                SeatClass = seat.Class.ToString(),
                FinalPrice = seat.Class == SeatClass.Executive ? flight.ExecutiveClassPrice : flight.EconomyClassPrice
            };
        }
    }
}
