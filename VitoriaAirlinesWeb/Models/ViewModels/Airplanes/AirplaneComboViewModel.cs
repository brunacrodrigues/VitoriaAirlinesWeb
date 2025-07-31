namespace VitoriaAirlinesWeb.Models.ViewModels.Airplanes
{
    /// <summary>
    /// Represents a simplified view model for an airplane, often used in combo boxes or dropdowns.
    /// </summary>
    public class AirplaneComboViewModel
    {
        /// <summary>
        /// Gets or sets the unique identifier for the airplane.
        /// </summary>
        public int Id { get; set; }


        /// <summary>
        /// Gets or sets the model name of the airplane.
        /// </summary>
        public string Model { get; set; } = null!;

        /// <summary>
        /// Gets or sets the total number of economy class seats.
        /// </summary>
        public int EconomySeats { get; set; }


        /// <summary>
        /// Gets or sets the total number of executive class seats.
        /// </summary>
        public int ExecutiveSeats { get; set; }
    }
}