﻿using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
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
                DepartureTime = flight.DepartureUtc.ToLocalTime(),
                ArrivalTime = flight.ArrivalUtc.ToLocalTime(),
                OldSeatInfo = $"{oldTicket.Seat.Row}{oldTicket.Seat.Letter}",
                OldSeatClass = oldTicket.Seat.Class.ToString(),
                NewSeatInfo = $"{newSeat.Row}{newSeat.Letter}",
                NewSeatClass = newSeat.Class.ToString(),
                OldPricePaid = oldPrice,
                NewPrice = newPrice,
                PriceDifference = priceDifference
            };
        }

        public FlightDashboardViewModel ToFlightDashboardViewModel(Flight entity)
        {
            return new FlightDashboardViewModel
            {
                Id = entity.Id,
                FlightNumber = entity.FlightNumber,

                OriginAirportFullName = entity.OriginAirport.FullName,
                OriginCountryFlagUrl = entity.OriginAirport.Country?.FlagImageUrl ?? "",

                DestinationAirportFullName = entity.DestinationAirport.FullName,
                DestinationCountryFlagUrl = entity.DestinationAirport.Country?.FlagImageUrl ?? "",

                DepartureFormatted = entity.DepartureUtc.ToLocalTime().ToString("HH:mm dd MMM"),
                DepartureIso = entity.DepartureUtc.ToLocalTime().ToString("o")
            };
        }

        public void UpdateFlightFromViewModel(Flight entity, FlightViewModel model)
        {
            entity.OriginAirportId = model.OriginAirportId!.Value;
            entity.DestinationAirportId = model.DestinationAirportId!.Value;
            entity.AirplaneId = model.AirplaneId;
            entity.EconomyClassPrice = model.EconomyClassPrice;
            entity.ExecutiveClassPrice = model.ExecutiveClassPrice;
            entity.DepartureUtc = model.DepartureDate!.Value.ToDateTime(model.DepartureTime!.Value).ToUniversalTime();
            entity.Duration = model.Duration!.Value;
        }

        public List<FlightDisplayViewModel> ToFlightDisplayViewModel(IEnumerable<Flight> flights)
        {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Lisbon");

            return flights.Select(f => new FlightDisplayViewModel
            {
                Id = f.Id,
                FlightNumber = f.FlightNumber,
                AirplaneModel = f.Airplane.Model,
                Origin = f.OriginAirport.FullName,
                OriginFlagUrl = f.OriginAirport.Country?.FlagImageUrl ?? "",
                Destination = f.DestinationAirport.FullName,
                DestinationFlagUrl = f.DestinationAirport.Country?.FlagImageUrl ?? "",
                Departure = TimeZoneInfo.ConvertTimeFromUtc(f.DepartureUtc, timeZone).ToString("yyyy-MM-dd HH:mm"),
                Arrival = TimeZoneInfo.ConvertTimeFromUtc(f.ArrivalUtc, timeZone).ToString("yyyy-MM-dd HH:mm"),
                Duration = f.Duration.ToString(@"hh\:mm"),
                ExecutiveClassPrice = f.ExecutiveClassPrice,
                EconomyClassPrice = f.EconomyClassPrice,
                Status = f.Status.ToString()
            }).ToList();
        }

        public List<TicketDisplayViewModel> ToTicketDisplayViewModel(IEnumerable<Ticket> tickets)
        {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Lisbon");

            return tickets.Select(t => new TicketDisplayViewModel
            {
                Id = t.Id,
                FlightNumber = t.Flight.FlightNumber,
                Origin = t.Flight.OriginAirport.FullName,
                OriginFlagUrl = t.Flight.OriginAirport.Country?.FlagImageUrl ?? "",
                Destination = t.Flight.DestinationAirport.FullName,
                DestinationFlagUrl = t.Flight.DestinationAirport.Country?.FlagImageUrl ?? "",
                Departure = TimeZoneInfo.ConvertTimeFromUtc(t.Flight.DepartureUtc, timeZone),
                Arrival = TimeZoneInfo.ConvertTimeFromUtc(t.Flight.ArrivalUtc, timeZone),
                SeatDisplay = $"{t.Seat.Row}{t.Seat.Letter}",
                Class = t.Seat.Class.ToString(),
                PricePaid = t.PricePaid,
                Status = t.Flight.Status.ToString()

            }).ToList();
        }
    }
}
