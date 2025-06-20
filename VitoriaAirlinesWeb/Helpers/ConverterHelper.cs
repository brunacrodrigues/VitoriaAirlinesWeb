using VitoriaAirlinesWeb.Data.Entities;
using VitoriaAirlinesWeb.Data.Enums;
using VitoriaAirlinesWeb.Models.Airplane;
using VitoriaAirlinesWeb.Models.Airport;
using VitoriaAirlinesWeb.Models.Customer;
using VitoriaAirlinesWeb.Models.Flight;

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
                ImageId = imageId
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

        public AirportViewModel ToAirportViewModel(Airport airport)
        {
            return new AirportViewModel
            {
                Id = airport.Id,
                IATA = airport.IATA,
                Name = airport.Name,
                City = airport.City,
                CountryId = airport.CountryId
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

        public FlightViewModel ToFlightViewModel(Flight flight)
        {
            return new FlightViewModel
            {
                Id = flight.Id,
                FlightNumber = flight.FlightNumber,
                OriginAirportId = flight.OriginAirportId,
                DestinationAirportId = flight.DestinationAirportId,
                AirplaneId = flight.AirplaneId,
                EconomyClassPrice = flight.EconomyClassPrice,
                ExecutiveClassPrice = flight.ExecutiveClassPrice,
                DepartureDate = DateOnly.FromDateTime(flight.DepartureUtc.ToLocalTime()),
                DepartureTime = TimeOnly.FromDateTime(flight.DepartureUtc.ToLocalTime()),
                Duration = flight.Duration,
                Status = flight.Status
            };
        }

    }
}
