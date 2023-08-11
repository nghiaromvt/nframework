using System.Collections.Generic;
using System.Diagnostics;

namespace NFramework
{
    /// <summary>
    /// Use this class to run performance tests in your code. 
    /// It'll output the time spent between the StartTest and the EndTest calls
    /// Make sure to use a unique ID for both calls
    /// </summary>
    public static class SpeedTester
    {
        /// <summary>
        /// A struct to store data associated to speed tests
        /// </summary>
        public struct SpeedTestItem
        {
            /// the name of the test, has to be unique
            public string testID;
            /// a stopwatch to compute time
            public Stopwatch timer;

            /// <summary>
            /// Creates a speed test with the specified ID and starts the timer
            /// </summary>
            public SpeedTestItem(string testID)
            {
                this.testID = testID;
                timer = Stopwatch.StartNew();
            }
        }

        private static readonly Dictionary<string, SpeedTestItem> _speedTestDict = new Dictionary<string, SpeedTestItem>();

        /// <summary>
        /// Starts a speed test of the specified ID
        /// </summary>
        public static void StartTest(string testID)
        {
            if (_speedTestDict.ContainsKey(testID))
            {
                Logger.LogError($"TestID[{testID}] already exist and will be overrided");
                _speedTestDict.Remove(testID);
            }

            SpeedTestItem item = new SpeedTestItem(testID);
            _speedTestDict.Add(testID, item);
        }

        /// <summary>
        /// Stops a speed test of the specified ID
        /// </summary>
        public static void EndTest(string testID)
        {
            if (!_speedTestDict.ContainsKey(testID))
            {
                Logger.LogError($"TestID[{testID}] is not exist");
                return;
            }

            _speedTestDict[testID].timer.Stop();
            float elapsedTime = _speedTestDict[testID].timer.ElapsedMilliseconds / 1000f;
            _speedTestDict.Remove(testID);

            Logger.Log($"TestID[{testID}] end test with {elapsedTime}s");
        }
    }
}
