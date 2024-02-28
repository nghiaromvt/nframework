#if USE_ADJUST && USE_ADJUST_ANALYTICS
using com.adjust.sdk;
#endif
using UnityEngine;

namespace NFramework.Tracking
{
    public class AdjustAnalyticsAdapter : TrackingAdapterBase
    {
        [SerializeField] private string _appToken;
        [SerializeField] private AdjustEnvironment _environment;

        public override ETrackingAdapterType AdapterType => ETrackingAdapterType.Adjust;

#if USE_ADJUST && USE_ADJUST_ANALYTICS
        public override void Init(TrackingAdapterConfig config)
        {
            base.Init(config);
            Adjust.start(new AdjustConfig(_appToken, _environment));
            IsInitialized = true;
        }

        protected override void TrackEventSDK(string eventToken)
        {
            base.TrackEventSDK(eventToken);
            var adjustEvent = new AdjustEvent(eventToken);
            Adjust.trackEvent(adjustEvent);
        }

        public void TrackRevenue(string eventToken, double amount, string currency, string transactionId)
        {
            var adjustEvent = new AdjustEvent(eventToken);
            adjustEvent.setRevenue(amount, currency);
            adjustEvent.setTransactionId(transactionId);
            Adjust.trackEvent(adjustEvent);
        }
#endif
    }
}