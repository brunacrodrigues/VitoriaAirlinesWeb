namespace VitoriaAirlinesWeb.Models.ViewModels.Dashboard
{
    /// <summary>
    /// Represents a view model for displaying the least occupied airplane model.
    /// </summary>
    public class LeastOccupiedAirplaneViewModel
    {
        /// <summary>
        /// Gets or sets the model name of the least occupied airplane.
        /// </summary>
        public string Model { get; set; } = null!;


        /// <summary>
        /// Gets or sets the occupancy rate for this airplane model (as a percentage).
        /// </summary>
        public double OccupancyRate { get; set; }
    }
}