namespace VitoriaAirlinesWeb.Models.ViewModels.Dashboard
{
    /// <summary>
    /// Represents a view model for displaying ticket sales grouped by day.
    /// </summary>
    public class TicketSalesByDayViewModel
    {
        /// <summary>
        /// Gets or sets the formatted date for the sales data (e.g., "01 Jan").
        /// </summary>
        public string Date { get; set; } = null!;


        /// <summary>
        /// Gets or sets the number of tickets sold on this date.
        /// </summary>
        public int TicketCount { get; set; }
    }
}