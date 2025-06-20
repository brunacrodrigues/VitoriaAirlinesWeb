using VitoriaAirlinesWeb.Data.Entities;
using VitoriaAirlinesWeb.Models.Airplane;
using VitoriaAirlinesWeb.Models.Airport;
using VitoriaAirlinesWeb.Models.Customer;
using VitoriaAirlinesWeb.Models.Flight;

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
    }
}
