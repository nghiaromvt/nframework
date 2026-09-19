using System;

namespace NFramework
{
    public static class FriendlyGuid
    {
        private static readonly Random _random = new Random();

        // 8da18880a18a4cd
        public static string NewFromTicks() => DateTime.Now.Ticks.ToString("x");

        // c9a646d3-9c61-4cb7-bfcd-ee2522c8f633
        public static string NewFromGuid() => Guid.NewGuid().ToString();

        // 1WIXVZtbA0qKPcZ
        public static string NewFromGuidBase64() => Convert.ToBase64String(Guid.NewGuid().ToByteArray())
            .Replace("/", "_")
            .Replace("+", "-")
            .Substring(0, 15);

        // 1WIXVZtbA0qKPcZ
        public static string NewFromGuidShorted() => Guid.NewGuid().ToString("N").Substring(0, 15);

        // 2c6fec62
        public static string NewFromRandomInt() => _random.Next(int.MaxValue).ToString("x");

        // 127d9edf
        public static string NewFromRandomLong() => _random.Next().ToString("x");

        // 127d9edf14385adb
        public static string NewFromRandomDoubleLong() => $"{_random.Next():x}{_random.Next():x}";
    }
}
