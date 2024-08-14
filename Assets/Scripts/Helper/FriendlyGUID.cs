using System;

namespace NFramework
{
    public static class FriendlyGUID
    {
        private static System.Random _random = new Random();

        // 8da18880a18a4cd
        public static string NewId_FromTicks() => DateTime.Now.Ticks.ToString("x");

        // c9a646d3-9c61-4cb7-bfcd-ee2522c8f633
        public static string NewId_FromGuid() => Guid.NewGuid().ToString();

        // 1WIXVZtbA0qKPcZ
        public static string NewId_FromGuidBase64() => Convert.ToBase64String(Guid.NewGuid().ToByteArray())
            .Replace("/", "_")
            .Replace("+", "-")
            .Substring(0, 15);

        // 1WIXVZtbA0qKPcZ
        public static string NewId_FromGuidShorted() => Guid.NewGuid().ToString("N").Substring(0, 15);

        // 2c6fec62
        public static string NewId_FromRandomInt() => _random.Next(int.MaxValue).ToString("x");

        // 127d9edf
        public static string NewId_FromRandomLong() => _random.Next().ToString("x");

        // 127d9edf14385adb
        public static string NewId_FromRandomDoubleLong() => $"{_random.Next().ToString("x")}{_random.Next().ToString("x")}";
    }
}
