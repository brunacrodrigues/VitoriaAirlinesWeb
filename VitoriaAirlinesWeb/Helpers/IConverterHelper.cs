using VitoriaAirlinesWeb.Data.Entities;
using VitoriaAirlinesWeb.Models.Airports;
using VitoriaAirlinesWeb.Models.ViewModels.Airplanes;
using VitoriaAirlinesWeb.Models.ViewModels.Booking;
using VitoriaAirlinesWeb.Models.ViewModels.Customers;
using VitoriaAirlinesWeb.Models.ViewModels.Employees;
using VitoriaAirlinesWeb.Models.ViewModels.Flights;

namespace VitoriaAirlinesWeb.Helpers
{
    public interface IConverterHelper
    {
        void UpdateCustomerProfile(CustomerProfile entity, CustomerProfileViewModel model);

        void UpdateCustomerProfile(CustomerProfile entity, CustomerProfileAdminViewModel model);

        CustomerProfileViewModel ToCustomerProfileViewModel(CustomerProfile entity);

        CustomerProfileAdminViewModel ToCustomerProfileAdminViewModel(CustomerProfile entity);

        Airplane ToAirplane(AirplaneViewModel model, Guid imageId, bool isNew);

        AirplaneViewModel ToAirplaneViewModel(Airplane entity);

        Airport ToAirport(AirportViewModel model, bool isNew);

        AirportViewModel ToAirportViewModel(Airport entity);

        Flight ToFlight(FlightViewModel model, bool isNew);

        FlightViewModel ToFlightViewModel(Flight entity);

        User ToUser(EditEmployeeViewModel model, User user);

        EditEmployeeViewModel ToEditEmployeeViewModel(User entity);

        SelectSeatViewModel ToSelectSeatViewModelAsync(Flight enitty, IEnumerable<int> occupiedSeatsIds);

        ConfirmBookingViewModel ToConfirmBookingViewModel(Flight flight, Seat seat);

        ConfirmSeatChangeViewModel ToConfirmSeatChangeViewModel(Ticket oldTicket, Seat newSeat, decimal newPrice);
    }
}
