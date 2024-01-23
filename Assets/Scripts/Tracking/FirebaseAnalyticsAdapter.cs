#if USE_FIREBASE && USE_FIREBASE_ANALYTIC
using Firebase.Analytics;
using NFramework.Firebase;
using System.Collections.Generic;
using UnityEngine;

namespace NFramework.Tracking
{
    public class FirebaseAnalyticsAdapter
    {
        public static bool IsInitialized { get; private set; }

        public static void Init(string userId = null)
        {
            FirebaseServiceManager.CheckAndTryInit(() =>
            {
                IsInitialized = true;

                if (!string.IsNullOrEmpty(userId))
                    FirebaseAnalytics.SetUserId(userId);

                FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventLogin);
            });
        }

        public static void TrackEvent(string eventName, Dictionary<string, object> parameters)
        {
            if (!FirebaseServiceManager.IsInitialized)
                return;

            var fireBaseParameters = GetFirebaseParameters(parameters);
            FirebaseAnalytics.LogEvent(eventName, fireBaseParameters);

            var message = $"[FirebaseAnalytics] TrackEvent: {eventName}. Params:\n";
            foreach (var pair in parameters)
            {
                message += $"-{pair.Key}: {pair.Value}\n";
            }
            Logger.Log(message);
        }

        public static void TrackEvent(string eventName)
        {
            if (!FirebaseServiceManager.IsInitialized)
                return;

            FirebaseAnalytics.LogEvent(eventName);
            Logger.Log($"[FirebaseAnalytics] TrackEvent: {eventName}");
        }

        private static Parameter[] GetFirebaseParameters(Dictionary<string, object> parameters)
        {
            var firebaseParameters = new List<Parameter>();
            foreach (var pair in parameters)
            {
                switch (pair.Value)
                {
                    case int intValue:
                        firebaseParameters.Add(new Parameter(pair.Key, System.Convert.ToInt64(intValue)));
                        break;
                    case long longValue:
                        firebaseParameters.Add(new Parameter(pair.Key, longValue));
                        break;
                    case float floatValue:
                        firebaseParameters.Add(new Parameter(pair.Key, System.Convert.ToDouble(floatValue)));
                        break;
                    case double doubleValue:
                        firebaseParameters.Add(new Parameter(pair.Key, doubleValue));
                        break;
                    case string stringValue:
                        firebaseParameters.Add(new Parameter(pair.Key, stringValue));
                        break;
                    case bool boolValue:
                        firebaseParameters.Add(new Parameter(pair.Key, boolValue.ToString()));
                        break;
                    default:
                        Debug.LogError("Invalid type for key " + pair.Key + ": " + pair.Value);
                        break;
                }
            }
            return firebaseParameters.ToArray();
        }
    }
}
#endif
