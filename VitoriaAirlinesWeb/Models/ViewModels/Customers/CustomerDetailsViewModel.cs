using VitoriaAirlinesWeb.Data.Entities;

namespace VitoriaAirlinesWeb.Models.ViewModels.Customers
{
    public class CustomerDetailsViewModel
    {
        public CustomerProfile Customer { get; set; } = null!;
        public int TotalFlights { get; set; }
        public Flight? LastFlight { get; set; }
        public Flight? NextFlight { get; set; }
        public string Role { get; set; }

    }
}
