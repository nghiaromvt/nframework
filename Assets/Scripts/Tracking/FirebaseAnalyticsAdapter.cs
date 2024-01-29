#if USE_FIREBASE && USE_FIREBASE_ANALYTICS
using Firebase.Analytics;
#endif
using NFramework.FirebaseService;
using System.Collections.Generic;
using UnityEngine;

namespace NFramework.Tracking
{
    public class FirebaseAnalyticsAdapter : TrackingAdapterBase
    {
        public override ETrackingAdapterType AdapterType => ETrackingAdapterType.FirebaseAnalytics;

#if USE_FIREBASE && USE_FIREBASE_ANALYTICS
        public override void Init(TrackingAdapterConfig config)
        {
            base.Init(config);
            FirebaseServiceManager.CheckAndTryInit(() =>
            {
                IsInitialized = true;
                FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventLogin);
            });
        }

        protected override void TrackEventSDK(string eventName)
        {
            base.TrackEventSDK(eventName);
            FirebaseAnalytics.LogEvent(eventName);
        }

        protected override void TrackEventSDK(string eventName, Dictionary<string, object> parameters)
        {
            base.TrackEventSDK(eventName, parameters);
            var fireBaseParameters = GetFirebaseParameters(parameters);
            FirebaseAnalytics.LogEvent(eventName, fireBaseParameters);
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
#endif
    }
}
