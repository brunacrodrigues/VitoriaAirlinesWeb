namespace VitoriaAirlinesWeb.Models.ViewModels.Airplanes
{
    public class AirplaneComboViewModel
    {
        public int Id { get; set; }

        public string Model { get; set; } = null!;

        public int EconomySeats { get; set; }

        public int ExecutiveSeats { get; set; }
    }
}
