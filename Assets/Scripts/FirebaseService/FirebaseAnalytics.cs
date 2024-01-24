#if USE_FIREBASE && USE_FIREBASE_ANALYTICS
using Firebase.Analytics;
#endif
using System.Collections.Generic;
using UnityEngine;

namespace NFramework.FirebaseService
{
    public class FirebaseAnalytics
    {
        public static bool IsInitialized { get; private set; }

        public static void Init(string userId = null)
        {
#if USE_FIREBASE && USE_FIREBASE_ANALYTICS
            FirebaseServiceManager.CheckAndTryInit(() =>
            {
                IsInitialized = true;

                if (!string.IsNullOrEmpty(userId))
                    Firebase.Analytics.FirebaseAnalytics.SetUserId(userId);

                Firebase.Analytics.FirebaseAnalytics.LogEvent(Firebase.Analytics.FirebaseAnalytics.EventLogin);
            });
#endif
        }

        public static void TrackEvent(string eventName, Dictionary<string, object> parameters)
        {
            if (!FirebaseServiceManager.IsInitialized)
                return;

#if USE_FIREBASE && USE_FIREBASE_ANALYTICS
            var fireBaseParameters = GetFirebaseParameters(parameters);
            Firebase.Analytics.FirebaseAnalytics.LogEvent(eventName, fireBaseParameters);

            var message = $"[FirebaseAnalytics] TrackEvent: {eventName}. Params:\n";
            foreach (var pair in parameters)
            {
                message += $"-{pair.Key}: {pair.Value}\n";
            }
            Logger.Log(message);
#endif
        }

        public static void TrackEvent(string eventName)
        {
            if (!FirebaseServiceManager.IsInitialized)
                return;

#if USE_FIREBASE && USE_FIREBASE_ANALYTICS
            Firebase.Analytics.FirebaseAnalytics.LogEvent(eventName);
            Logger.Log($"[FirebaseAnalytics] TrackEvent: {eventName}");
#endif
        }

#if USE_FIREBASE && USE_FIREBASE_ANALYTICS
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
#endif
    }
}
