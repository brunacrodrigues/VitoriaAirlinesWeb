namespace VitoriaAirlinesWeb.Data.Enums
{
    /// <summary>
    /// Defines the types of trips a user can search for or book.
    /// </summary>
    public enum TripType
    {
        /// <summary>
        /// A single journey from an origin to a destination.
        /// </summary>
        OneWay,
        /// <summary>
        /// A journey from an origin to a destination and back to the origin.
        /// </summary>
        RoundTrip
    }
}