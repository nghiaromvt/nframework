using NFramework.Ads;
using NFramework.IAP;
using System.Collections.Generic;
using System.Linq;

namespace NFramework.Tracking
{
    public class TrackingManager : SingletonMono<TrackingManager>
    {
        public Dictionary<ETrackingAdapterType, TrackingAdapterBase> AdapterDic { get; private set; } = new Dictionary<ETrackingAdapterType, TrackingAdapterBase>();

        protected override void Awake()
        {
            base.Awake();
            foreach (var adapter in GetComponentsInChildren<TrackingAdapterBase>())
                AdapterDic.Add(adapter.AdapterType, adapter);
        }

        public void Init(TrackingAdapterConfig config = null, ETrackingAdapterType specificAdapterType = ETrackingAdapterType.None)
        {
            if (DeviceInfo.IsNoTracking)
                return;

            if (config is null)
                config = new TrackingAdapterConfig();

            if (specificAdapterType == ETrackingAdapterType.None)
            {
                foreach (var adapter in AdapterDic.Values)
                {
                    if (!adapter.IsInitialized)
                        adapter.Init(config);
                }
            }
            else
            {
                if (TryGetAdapter(specificAdapterType, out var adapter) && !adapter.IsInitialized)
                    adapter.Init(config);
            }
        }

        public void TrackEvent(string eventName, ETrackingAdapterType specificAdapterType = ETrackingAdapterType.None)
        {
            if (DeviceInfo.IsNoTracking)
                return;

            if (specificAdapterType == ETrackingAdapterType.None)
            {
                foreach (var adapter in AdapterDic.Values)
                    adapter.TrackEvent(eventName);
            }
            else
            {
                if (TryGetAdapter(specificAdapterType, out var adapter))
                    adapter.TrackEvent(eventName);
            }
        }

        public void TrackEvent(string eventName, Dictionary<string, object> parameters, ETrackingAdapterType specificAdapterType = ETrackingAdapterType.None)
        {
            if (DeviceInfo.IsNoTracking)
                return;

            if (specificAdapterType == ETrackingAdapterType.None)
            {
                foreach (var adapter in AdapterDic.Values)
                    adapter.TrackEvent(eventName, parameters);
            }
            else
            {
                if (TryGetAdapter(specificAdapterType, out var adapter))
                    adapter.TrackEvent(eventName, parameters);
            }
        }

        public void TrackAdImpression(string eventName, AdsRevenueData data, ETrackingAdapterType specificAdapterType = ETrackingAdapterType.None)
        {
            if (DeviceInfo.IsNoTracking)
                return;

            if (specificAdapterType == ETrackingAdapterType.None)
            {
                foreach (var adapter in AdapterDic.Values)
                    adapter.TrackAdImpression(eventName, data);
            }
            else
            {
                if (TryGetAdapter(specificAdapterType, out var adapter))
                    adapter.TrackAdImpression(eventName, data);
            }
        }

        public void TrackIAP(string eventName, IAPRevenueData data, ETrackingAdapterType specificAdapterType = ETrackingAdapterType.None)
        {
            if (DeviceInfo.IsNoTracking)
                return;

            if (specificAdapterType == ETrackingAdapterType.None)
            {
                foreach (var adapter in AdapterDic.Values)
                    adapter.TrackIAP(eventName, data);
            }
            else
            {
                if (TryGetAdapter(specificAdapterType, out var adapter))
                    adapter.TrackIAP(eventName, data);
            }
        }

        public bool TryGetAdapter(ETrackingAdapterType specificAdapterType, out TrackingAdapterBase adapter)
        {
            adapter = null;
            if (AdapterDic.Count == 0)
            {
                Logger.LogError($"No adapter", this);
                return false;
            }

            if (specificAdapterType != ETrackingAdapterType.None)
            {
                if (AdapterDic.TryGetValue(specificAdapterType, out adapter))
                {
                    return true;
                }
                else
                {
                    Logger.LogError($"Not found adapter with type: {specificAdapterType}", this);
                    return false;
                }
            }
            else
            {
                adapter = AdapterDic.Values.First();
                return true;
            }
        }
    }

    public enum ETrackingAdapterType
    {
        None,
        FirebaseAnalytics,
        AppsFlyer,
        Adjust,
    }

    [System.Serializable]
    public class TrackingAdapterConfig
    {

    }
}
