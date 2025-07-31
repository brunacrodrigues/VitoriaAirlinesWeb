namespace VitoriaAirlinesWeb.Data.Enums
{
    /// <summary>
    /// Defines the possible operational statuses for a flight.
    /// </summary>
    public enum FlightStatus
    {
        /// <summary>
        /// The flight is planned and pending departure.
        /// </summary>
        Scheduled,
        /// <summary>
        /// The flight has been canceled.
        /// </summary>
        Canceled,
        /// <summary>
        /// The flight has departed from its origin.
        /// </summary>
        Departed,
        /// <summary>
        /// The flight has successfully arrived at its destination.
        /// </summary>
        Completed
    }
}
