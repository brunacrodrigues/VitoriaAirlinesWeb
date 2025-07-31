namespace VitoriaAirlinesWeb.Data.Enums
{
    /// <summary>
    /// Defines the possible operational statuses for an airplane.
    /// </summary>
    public enum AirplaneStatus
    {
        /// <summary>
        /// The airplane is operational and available for flights.
        /// </summary>
        Active,

        /// <summary>
        /// The airplane is not operational and not available for flights (e.g., retired, long-term storage).
        /// </summary>
        Inactive,

        /// <summary>
        /// The airplane is currently undergoing maintenance and is not available for flights.
        /// </summary>
        Maintenance
    }
}
