namespace VitoriaAirlinesWeb.Helpers
{
    public static class TimezoneHelper
    {
        private const string DefaultTimeZoneId = "GMT Standard Time";

        public static DateTime ConvertToUtc(DateTime localDateTime, string timeZoneId = DefaultTimeZoneId)
        {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);

            // Garante que a hora é interpretada no fuso correto
            localDateTime = DateTime.SpecifyKind(localDateTime, DateTimeKind.Unspecified);

            return TimeZoneInfo.ConvertTimeToUtc(localDateTime, timeZone);
        }

        public static DateTime ConvertToLocal(DateTime utcDateTime, string timeZoneId = DefaultTimeZoneId)
        {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, timeZone);
        }
    }
}
