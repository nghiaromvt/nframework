using System;

namespace NFramework
{
    public static class TimeHelper
    {
        public static DateTime ConvertUnixTimeToDateTime(long unixTime) => DateTime.UnixEpoch.AddSeconds(unixTime);

        public static long ConvertDateTimeToUnixTime(DateTime dateTime) =>
            ((DateTimeOffset)dateTime).ToUnixTimeSeconds();

        public static DateTime GetCurrentTime(bool utc = false) => utc ? DateTime.UtcNow : DateTime.Now;

        public static TimeSpan GetCurrentTimeSpan(bool utc = false) => GetCurrentTime(utc).Subtract(DateTime.MinValue);
    }
}