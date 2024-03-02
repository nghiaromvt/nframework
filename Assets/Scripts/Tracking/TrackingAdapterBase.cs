using NFramework.Ads;
using NFramework.IAP;
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

        protected virtual void TrackEventSDK(string eventName) => Logger.LogError("Need override", this);

        public void TrackEvent(string eventName, Dictionary<string, object> parameters)
        {
            if (IsInitialized)
            {
                TrackEventSDK(eventName, parameters);
                Log(eventName, parameters);
            }
        }

        protected virtual void TrackEventSDK(string eventName, Dictionary<string, object> parameters) => Logger.LogError("Need override", this);

        public void TrackAdImpression(string eventName, AdsRevenueData data)
        {
            if (IsInitialized)
                TrackAdImpressionSDK(eventName, data);
        }

        protected virtual void TrackAdImpressionSDK(string eventName, AdsRevenueData data) => Logger.LogError("Need override", this);

        public void TrackIAP(string eventName, IAPRevenueData data) 
        {
            if (IsInitialized)
                TrackIAPSDK(eventName, data);
        }

        protected virtual void TrackIAPSDK(string eventName, IAPRevenueData data) => Logger.LogError("Need override", this);

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
