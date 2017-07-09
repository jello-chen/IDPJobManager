namespace IDPJobManager.Core.Extensions
{
    using System;

    public static class DateTimeOffsetExtensions
    {
        public static long UnixTicks(this DateTime dt)
        {
            var unixTimestampOrigin = new DateTime(1970, 1, 1);
            return (long) new TimeSpan(dt.Ticks - unixTimestampOrigin.Ticks).TotalMilliseconds;
        }

        public static DateTime? ToDateTime(this DateTimeOffset? offset)
        {
            if (offset.HasValue)
            {
                return offset.Value.DateTime;
            }

            return null;
        }

        public static DateTime GetStartDateTimeOfDay(this DateTime dt)
        {
            return new DateTime(dt.Year, dt.Month, dt.Day);
        }

        public static DateTime GetEndDateTimeOfDay(this DateTime dt)
        {
            return new DateTime(dt.Year, dt.Month, dt.Day, 23, 59, 59, 999);
        }
    }
}