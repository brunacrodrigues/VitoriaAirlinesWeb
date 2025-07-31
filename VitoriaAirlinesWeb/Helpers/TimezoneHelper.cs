namespace VitoriaAirlinesWeb.Helpers
{
    /// <summary>
    /// Provides static helper methods for converting DateTime objects between UTC and a default local timezone.
    /// </summary>
    public static class TimezoneHelper
    {
        private const string DefaultTimeZoneId = "GMT Standard Time";


        /// <summary>
        /// Converts a local DateTime (interpreted in the default timezone) to its equivalent Coordinated Universal Time (UTC).
        /// Explicitly sets the DateTimeKind to Unspecified before conversion to ensure correct interpretation by TimeZoneInfo.
        /// </summary>
        /// <param name="localDateTime">The local DateTime to convert.</param>
        /// <param name="timeZoneId">Optional: The ID of the timezone to interpret localDateTime in. Defaults to "GMT Standard Time".</param>
        /// <returns>The DateTime converted to UTC.</returns>
        public static DateTime ConvertToUtc(DateTime localDateTime, string timeZoneId = DefaultTimeZoneId)
        {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);

            // Ensure the DateTime is treated as Unspecified so TimeZoneInfo can correctly convert it
            // from the specified timezone to UTC.
            localDateTime = DateTime.SpecifyKind(localDateTime, DateTimeKind.Unspecified);

            return TimeZoneInfo.ConvertTimeToUtc(localDateTime, timeZone);
        }


        /// <summary>
        /// Converts a UTC DateTime to its equivalent local time in the default timezone.
        /// </summary>
        /// <param name="utcDateTime">The UTC DateTime to convert.</param>
        /// <param name="timeZoneId">Optional: The ID of the timezone to convert to. Defaults to "GMT Standard Time".</param>
        /// <returns>The DateTime converted to local time.</returns>
        public static DateTime ConvertToLocal(DateTime utcDateTime, string timeZoneId = DefaultTimeZoneId)
        {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, timeZone);
        }
    }
}