using VitoriaAirlinesWeb.Data.Entities;
using VitoriaAirlinesWeb.Models.Airplane;
using VitoriaAirlinesWeb.Models.Customer;

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
    }
}
