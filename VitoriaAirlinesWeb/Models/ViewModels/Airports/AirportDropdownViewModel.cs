namespace VitoriaAirlinesWeb.Models.Airports
{
    /// <summary>
    /// Represents a view model for an airport used in dropdowns, including display text, value, and an icon (e.g., flag).
    /// </summary>
    public class AirportDropdownViewModel
    {
        /// <summary>
        /// Gets or sets the display text for the dropdown item (e.g., "Paris - Charles de Gaulle").
        /// </summary>
        public string Text { get; set; } = null!; // Assumes initialization


        /// <summary>
        /// Gets or sets the value associated with the dropdown item (e.g., the airport ID).
        /// </summary>
        public string Value { get; set; } = null!; // Assumes initialization


        /// <summary>
        /// Gets or sets the icon identifier, typically a country code for a flag icon.
        /// </summary>
        public string Icon { get; set; } = null!; // Assumes initialization
    }
}