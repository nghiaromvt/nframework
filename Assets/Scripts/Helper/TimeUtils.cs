using System;

namespace NFramework
{
    public class TimeUtils
    {
        public static string SecondsToTimeString(double seconds)
        {
            var timeSpan = TimeSpan.FromSeconds(seconds);
            var totalHours = timeSpan.TotalHours;
            var remainingHours = (int)totalHours % 24;
            return string.Format("{0:00}:{1:00}:{2:00}", remainingHours, timeSpan.Minutes, timeSpan.Seconds);
        }

        public static DateTime GetCurrentTime(bool utc = false) => utc ? DateTime.UtcNow : DateTime.Now;

        public static TimeSpan GetCurrentTimeSpan(bool utc = false) => GetCurrentTime(utc).Subtract(DateTime.MinValue);

        public static DateTime FromEpochTime(long sec)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(sec);
        }

        public static long ToEpochTime(DateTime time)
        {
            return (long)(time - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
        }
    }
}
