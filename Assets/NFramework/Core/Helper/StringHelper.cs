using System;

namespace NFramework
{
    public static class StringHelper
    {
        public static string FormatTime(double totalSeconds)
        {
            if (totalSeconds < 0) totalSeconds = 0;

            TimeSpan time = TimeSpan.FromSeconds(totalSeconds);
            return FormatTime(time);
        }

        public static string FormatTime(TimeSpan time)
        {
            if (time.TotalDays >= 1)
            {
                return $"{(int)time.TotalDays}d {time.Hours}h";
            }
            else if (time.TotalHours >= 1)
            {
                return $"{time.Hours}h {time.Minutes}m";
            }
            else
            {
                return $"{time.Minutes}m {time.Seconds}s";
            }
        }
    }
}