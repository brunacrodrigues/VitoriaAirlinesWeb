namespace VitoriaAirlinesWeb.Models.ViewModels.Dashboard
{
    /// <summary>
    /// Represents a simplified view model for displaying airplane occupancy rates.
    /// </summary>
    public class AirplaneOccupancyViewModel
    {
        /// <summary>
        /// Gets or sets the model name of the airplane.
        /// </summary>
        public string Model { get; set; } = null!;


        /// <summary>
        /// Gets or sets the calculated occupancy rate for the airplane model (as a percentage).
        /// </summary>
        public double OccupancyRate { get; set; }
    }
}