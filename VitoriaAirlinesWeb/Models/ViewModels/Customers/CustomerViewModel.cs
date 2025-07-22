namespace VitoriaAirlinesWeb.Models.ViewModels.Customers
{
    public class CustomerViewModel
    {
        public int Id { get; set; }
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? CountryName { get; set; }
        public string? CountryFlagUrl { get; set; }
        public string? PassportNumber { get; set; }
    }
}
