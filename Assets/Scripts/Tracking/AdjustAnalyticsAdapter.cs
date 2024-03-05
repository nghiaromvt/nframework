#if USE_ADJUST_ANALYTICS
using com.adjust.sdk;
using NFramework.Ads;
using NFramework.IAP;
using System.Collections.Generic;
#endif
using UnityEngine;

namespace NFramework.Tracking
{
    public class AdjustAnalyticsAdapter : TrackingAdapterBase
    {
        [SerializeField] private string _appToken;
#if USE_ADJUST_ANALYTICS
        [SerializeField] private AdjustEnvironment _environment;
#endif

        public override ETrackingAdapterType AdapterType => ETrackingAdapterType.Adjust;

#if USE_ADJUST_ANALYTICS
        public override void Init(TrackingAdapterConfig config)
        {
            base.Init(config);

            var adjustConfig = new AdjustConfig(_appToken, _environment);
            if (DeviceInfo.IsDevelopment)
                adjustConfig.setLogLevel(AdjustLogLevel.Verbose);

            Adjust.start(adjustConfig);
            IsInitialized = true;
        }

        protected override void TrackEventSDK(string eventName)
        {
            var adjustEvent = new AdjustEvent(eventName);
            Adjust.trackEvent(adjustEvent);
        }

        protected override void TrackAdImpressionSDK(string eventName, AdsRevenueData data)
        {
            AdjustAdRevenue adjustAdRevenue = new AdjustAdRevenue(AdjustConfig.AdjustAdRevenueSourceIronSource);
            adjustAdRevenue.setRevenue(data.value, data.currency);
            Adjust.trackAdRevenue(adjustAdRevenue);
            Log(AdjustConfig.AdjustAdRevenueSourceIronSource, new Dictionary<string, object>
            {
                { "currency", data.currency },
                { "value", data.value }
            });
        }

        protected override void TrackIAPSDK(string eventName, IAPRevenueData data)
        {
            AdjustEvent adjustEvent = new AdjustEvent(eventName);
            adjustEvent.setRevenue((double)data.price, data.currency);
            adjustEvent.setTransactionId(data.transactionID);
            Adjust.trackEvent(adjustEvent);
        }

        public void TrackRevenue(string eventName, double value, string currency)
        {
            if (IsInitialized)
            {
                AdjustEvent adjustEvent = new AdjustEvent(eventName);
                adjustEvent.setRevenue((double)value, currency);
                Adjust.trackEvent(adjustEvent);
            }
        }
#endif
    }
}