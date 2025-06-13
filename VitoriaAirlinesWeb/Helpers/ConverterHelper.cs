using VitoriaAirlinesWeb.Data.Entities;
using VitoriaAirlinesWeb.Models.Customer;

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
    }
}
