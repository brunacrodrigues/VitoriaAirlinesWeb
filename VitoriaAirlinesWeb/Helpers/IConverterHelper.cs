using VitoriaAirlinesWeb.Data.Entities;
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
    /// Defines the contract for a helper class that converts between data entities and view models,
    /// and updates entities from view model data.
    /// </summary>
    public interface IConverterHelper
    {
        /// <summary>
        /// Updates a CustomerProfile entity with data from a CustomerProfileViewModel.
        /// </summary>
        /// <param name="entity">The CustomerProfile entity to update.</param>
        /// <param name="model">The CustomerProfileViewModel containing the new data.</param>
        void UpdateCustomerProfile(CustomerProfile entity, CustomerProfileViewModel model);


        /// <summary>
        /// Updates a CustomerProfile entity with data from a CustomerProfileAdminViewModel.
        /// </summary>
        /// <param name="entity">The CustomerProfile entity to update.</param>
        /// <param name="model">The CustomerProfileAdminViewModel containing the new data.</param>
        void UpdateCustomerProfile(CustomerProfile entity, CustomerProfileAdminViewModel model);


        /// <summary>
        /// Converts a CustomerProfile entity and a User entity to a CustomerProfileViewModel.
        /// </summary>
        /// <param name="profile">The CustomerProfile entity.</param>
        /// <param name="user">The User entity.</param>
        /// <returns>A new CustomerProfileViewModel.</returns>
        CustomerProfileViewModel ToCustomerProfileViewModel(CustomerProfile profile, User user);


        /// <summary>
        /// Converts a CustomerProfile entity to a CustomerProfileAdminViewModel.
        /// </summary>
        /// <param name="entity">The CustomerProfile entity to convert.</param>
        /// <returns>A new CustomerProfileAdminViewModel.</returns>
        CustomerProfileAdminViewModel ToCustomerProfileAdminViewModel(CustomerProfile entity);


        /// <summary>
        /// Converts an AirplaneViewModel to an Airplane entity.
        /// </summary>
        /// <param name="model">The AirplaneViewModel to convert.</param>
        /// <param name="imageId">The GUID of the airplane's image.</param>
        /// <param name="isNew">A flag indicating if this is a new airplane (true) or an existing one (false).</param>
        /// <returns>A new Airplane entity.</returns>
        Airplane ToAirplane(AirplaneViewModel model, Guid imageId, bool isNew);


        /// <summary>
        /// Converts an Airplane entity to an AirplaneViewModel.
        /// </summary>
        /// <param name="entity">The Airplane entity to convert.</param>
        /// <returns>A new AirplaneViewModel.</returns>
        AirplaneViewModel ToAirplaneViewModel(Airplane entity);


        /// <summary>
        /// Converts an AirportViewModel to an Airport entity.
        /// </summary>
        /// <param name="model">The AirportViewModel to convert.</param>
        /// <param name="isNew">A flag indicating if this is a new airport (true) or an existing one (false).</param>
        /// <returns>A new Airport entity.</returns>
        Airport ToAirport(AirportViewModel model, bool isNew);


        /// <summary>
        /// Converts an Airport entity to an AirportViewModel.
        /// </summary>
        /// <param name="entity">The Airport entity to convert.</param>
        /// <returns>A new AirportViewModel.</returns>
        AirportViewModel ToAirportViewModel(Airport entity);


        /// <summary>
        /// Converts a FlightViewModel to a Flight entity.
        /// </summary>
        /// <param name="model">The FlightViewModel to convert.</param>
        /// <param name="isNew">A flag indicating if this is a new flight (true) or an existing one (false).</param>
        /// <returns>A new Flight entity.</returns>
        Flight ToFlight(FlightViewModel model, bool isNew);


        /// <summary>
        /// Converts a Flight entity to a FlightViewModel.
        /// </summary>
        /// <param name="entity">The Flight entity to convert.</param>
        /// <returns>A new FlightViewModel.</returns>
        FlightViewModel ToFlightViewModel(Flight entity);


        /// <summary>
        /// Updates a User entity with data from an EditEmployeeViewModel.
        /// </summary>
        /// <param name="model">The EditEmployeeViewModel containing the new data.</param>
        /// <param name="user">The User entity to update.</param>
        /// <returns>The updated User entity.</returns>
        User ToUser(EditEmployeeViewModel model, User user);


        /// <summary>
        /// Converts a User entity to an EditEmployeeViewModel.
        /// </summary>
        /// <param name="entity">The User entity to convert.</param>
        /// <returns>A new EditEmployeeViewModel.</returns>
        EditEmployeeViewModel ToEditEmployeeViewModel(User entity);


        /// <summary>
        /// Converts a Flight entity and a list of occupied seat IDs to a SelectSeatViewModel.
        /// </summary>
        /// <param name="entity">The Flight entity.</param>
        /// <param name="occupiedSeatsIds">A collection of IDs for occupied seats on the flight.</param>
        /// <returns>A new SelectSeatViewModel.</returns>
        SelectSeatViewModel ToSelectSeatViewModel(Flight enitty, IEnumerable<int> occupiedSeatsIds);


        /// <summary>
        /// Converts a Flight entity and a Seat entity to a ConfirmBookingViewModel.
        /// </summary>
        /// <param name="flight">The Flight entity.</param>
        /// <param name="seat">The Seat entity.</param>
        /// <returns>A new ConfirmBookingViewModel.</returns>
        ConfirmBookingViewModel ToConfirmBookingViewModel(Flight flight, Seat seat);


        /// <summary>
        /// Converts old ticket details and a new seat to a ConfirmSeatChangeViewModel.
        /// </summary>
        /// <param name="oldTicket">The original Ticket entity.</param>
        /// <param name="newSeat">The newly selected Seat entity.</param>
        /// <param name="newPrice">The price of the new seat.</param>
        /// <returns>A new ConfirmSeatChangeViewModel.</returns>
        ConfirmSeatChangeViewModel ToConfirmSeatChangeViewModel(Ticket oldTicket, Seat newSeat, decimal newPrice);


        /// <summary>
        /// Converts a Flight entity to a FlightDashboardViewModel.
        /// </summary>
        /// <param name="entity">The Flight entity to convert.</param>
        /// <returns>A new FlightDashboardViewModel.</returns>
        FlightDashboardViewModel ToFlightDashboardViewModel(Flight entity);


        /// <summary>
        /// Updates a Flight entity with data from a FlightViewModel.
        /// </summary>
        /// <param name="entity">The Flight entity to update.</param>
        /// <param name="model">The FlightViewModel containing the new data.</param>
        void UpdateFlightFromViewModel(Flight entity, FlightViewModel model);


        /// <summary>
        /// Converts a collection of Flight entities to a list of FlightDisplayViewModel.
        /// </summary>
        /// <param name="flights">The collection of Flight entities to convert.</param>
        /// <returns>A list of new FlightDisplayViewModel objects.</returns>
        List<FlightDisplayViewModel> ToFlightDisplayViewModel(IEnumerable<Flight> flights);


        /// <summary>
        /// Converts a collection of Ticket entities to a list of TicketDisplayViewModel.
        /// </summary>
        /// <param name="tickets">The collection of Ticket entities to convert.</param>
        /// <returns>A list of new TicketDisplayViewModel objects.</returns>
        List<TicketDisplayViewModel> ToTicketDisplayViewModel(IEnumerable<Ticket> tickets);
    }
}