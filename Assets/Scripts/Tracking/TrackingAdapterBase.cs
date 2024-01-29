using NFramework.Ads;
using System.Collections.Generic;
using UnityEngine;

namespace NFramework.Tracking
{
    public class TrackingAdapterBase : MonoBehaviour
    {
        protected TrackingAdapterConfig _config;

        public virtual ETrackingAdapterType AdapterType => ETrackingAdapterType.None;
        public bool IsInitialized { get; protected set; }

        public virtual void Init(TrackingAdapterConfig config) => _config = config;

        public void TrackEvent(string eventName)
        {
            if (IsInitialized)
            {
                TrackEventSDK(eventName);
                Log(eventName);
            }
        }

        protected virtual void TrackEventSDK(string eventName) { }

        public void TrackEvent(string eventName, Dictionary<string, object> parameters)
        {
            if (IsInitialized)
            {
                TrackEventSDK(eventName, parameters);
                Log(eventName, parameters);
            }
        }

        protected virtual void TrackEventSDK(string eventName, Dictionary<string, object> parameters) { }

        public void TrackAdImpression(AdsRevenueData data)
        {
            TrackEvent("ad_impression", new Dictionary<string, object>
            {
                { "ad_platform", data.adPlatform },
                { "ad_source", data.adSource },
                { "ad_unit_name", data.adUnitName },
                { "ad_format", data.adFormat },
                { "currency", data.currency },
                { "value", data.value }
            });
        }

        protected void Log(string eventName, Dictionary<string, object> parameters = null)
        {
            var message = $"TrackEvent:{eventName}";
            if (parameters != null)
            {
                message += " - Params:\n";
                foreach (var pair in parameters)
                    message += $"-{pair.Key}: {pair.Value}\n";
            }
            Logger.Log(message, this);
        }
    }
}
