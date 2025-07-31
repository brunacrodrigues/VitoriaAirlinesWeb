namespace VitoriaAirlinesWeb.Models.ViewModels.Dashboard
{
    /// <summary>
    /// Represents a view model for displaying the most active airplane.
    /// </summary>
    public class MostActiveAirplaneViewModel
    {
        /// <summary>
        /// Gets or sets the model name of the most active airplane.
        /// </summary>
        public string Model { get; set; } = null!;


        /// <summary>
        /// Gets or sets the total number of flights associated with this airplane model.
        /// </summary>
        public int FlightCount { get; set; }
    }


}