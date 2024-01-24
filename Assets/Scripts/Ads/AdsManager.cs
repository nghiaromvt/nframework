using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NFramework.Ads
{
    public class AdsManager : SingletonMono<AdsManager>, ISaveable
    {
        public static event Action<bool> OnIsRemoveAdsChanged;

        [SerializeField] private SaveData _saveData;
        [SerializeField] private EAdsBannerPosition _defaultBannerPosition = EAdsBannerPosition.BottomCenter;

        private bool _isInitialized;

        public Dictionary<EAdsAdapterType, AdsAdapterBase> AdapterDic { get; private set; } = new Dictionary<EAdsAdapterType, AdsAdapterBase>();
        public bool IsInitialized => _isInitialized && !AdapterDic.Values.Any(x => !x.IsInitialized);
        public bool IsFullscreenAdShowing => AdapterDic.Values.Any(x => x.IsFullscreenAdShowing);

        public bool IsRemoveAds
        {
            get => _saveData.isRemoveAds;
            set
            {
                if (_saveData.isRemoveAds != value)
                {
                    _saveData.isRemoveAds = value;
                    DataChanged = true;
                    SaveManager.I.Save();

                    if (value)
                        DestroyBanner(true);

                    OnIsRemoveAdsChanged?.Invoke(value);
                }
            }
        }

        public void Init(IAdsCallbackListener adsCallbackListener = null)
        {
            if (DeviceInfo.IsNoAds || _isInitialized)
                return;

            _isInitialized = true;
            var config = new AdsInitConfig { bannerPosition = _defaultBannerPosition };
            foreach (var adapter in GetComponentsInChildren<AdsAdapterBase>())
            {
                adapter.Init(config, adsCallbackListener);
                AdapterDic.Add(adapter.AdapterType, adapter);
            }
        }

        public void LoadInter(bool forceAll = true, EAdsAdapterType specificAdapterType = EAdsAdapterType.None)
        {
            if (IsRemoveAds || DeviceInfo.IsNoAds)
                return;

            if (forceAll)
            {
                foreach (var adapter in AdapterDic.Values)
                {
                    if (adapter.AdsTypeUse.HasFlag(EAdsType.Banner))
                        adapter.LoadInter();
                }
            }
            else
            {
                if (TryGetAdapter(specificAdapterType, out var adapter))
                    adapter.LoadInter();
            }
        }

        public void ShowInter(AdsShowData data = null, EAdsAdapterType specificAdapterType = EAdsAdapterType.None)
        {
            if (IsRemoveAds || DeviceInfo.IsNoAds)
            {
                data?.callback?.Invoke(true);
                return;
            }

            if (TryGetAdapter(specificAdapterType, out var adapter))
                adapter.ShowInter(data);
            else
                data?.callback?.Invoke(false);
        }

        public void LoadReward(bool forceAll = true, EAdsAdapterType specificAdapterType = EAdsAdapterType.None)
        {
            if (IsRemoveAds || DeviceInfo.IsNoAds)
                return;

            if (forceAll)
            {
                foreach (var adapter in AdapterDic.Values)
                {
                    if (adapter.AdsTypeUse.HasFlag(EAdsType.Banner))
                        adapter.LoadReward();
                }
            }
            else
            {
                if (TryGetAdapter(specificAdapterType, out var adapter))
                    adapter.LoadReward();
            }
        }

        public void ShowReward(AdsShowData data = null, EAdsAdapterType specificAdapterType = EAdsAdapterType.None)
        {
            if (IsRemoveAds || DeviceInfo.IsNoAds)
            {
                data?.callback?.Invoke(true);
                return;
            }

            if (TryGetAdapter(specificAdapterType, out var adapter))
                adapter.ShowReward(data);
            else
                data?.callback?.Invoke(false);
        }

        public void LoadBanner(bool forceAll = true, EAdsAdapterType specificAdapterType = EAdsAdapterType.None)
        {
            if (IsRemoveAds || DeviceInfo.IsNoAds)
                return;

            if (forceAll)
            {
                foreach (var adapter in AdapterDic.Values)
                {
                    if (adapter.AdsTypeUse.HasFlag(EAdsType.Banner))
                        adapter.LoadBanner();
                }
            }
            else
            {
                if (TryGetAdapter(specificAdapterType, out var adapter))
                    adapter.LoadBanner();
            }
        }

        public void ShowBanner(bool forceAll = true, EAdsAdapterType specificAdapterType = EAdsAdapterType.None)
        {
            if (IsRemoveAds || DeviceInfo.IsNoAds)
                return;

            if (forceAll)
            {
                foreach (var adapter in AdapterDic.Values)
                {
                    if (adapter.AdsTypeUse.HasFlag(EAdsType.Banner))
                        adapter.ShowBanner();
                }
            }
            else
            {
                if (TryGetAdapter(specificAdapterType, out var adapter))
                    adapter.ShowBanner();
            }
        }

        public void HideBanner(bool forceAll = true, EAdsAdapterType specificAdapterType = EAdsAdapterType.None)
        {
            if (forceAll)
            {
                foreach (var adapter in AdapterDic.Values)
                {
                    if (adapter.AdsTypeUse.HasFlag(EAdsType.Banner))
                        adapter.HideBanner();
                }
            }
            else
            {
                if (TryGetAdapter(specificAdapterType, out var adapter))
                    adapter.HideBanner();
            }
        }

        public void DestroyBanner(bool forceAll = true, EAdsAdapterType specificAdapterType = EAdsAdapterType.None)
        {
            if (forceAll)
            {
                foreach (var adapter in AdapterDic.Values)
                {
                    if (adapter.AdsTypeUse.HasFlag(EAdsType.Banner))
                        adapter.DestroyBanner();
                }
            }
            else
            {
                if (TryGetAdapter(specificAdapterType, out var adapter))
                    adapter.HideBanner();
            }
        }

        private bool TryGetAdapter(EAdsAdapterType specificAdapterType, out AdsAdapterBase adapter)
        {
            adapter = null;
            if (AdapterDic.Count == 0)
            {
                Logger.LogError($"No adapter", this);
                return false;
            }

            if (specificAdapterType != EAdsAdapterType.None)
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

        #region ISaveable
        [Serializable]
        public class SaveData
        {
            public bool isRemoveAds;
        }

        public string SaveKey => "AdsManager";

        public bool DataChanged { get; set; }

        public object GetData() => _saveData;

        public void SetData(string data)
        {
            if (string.IsNullOrEmpty(data))
            {
                _saveData = new SaveData();
                DataChanged = true;
            }
            else
            {
                _saveData = JsonUtility.FromJson<SaveData>(data);
            }
        }

        public void OnAllDataLoaded() { }
        #endregion
    }

    public enum EAdsAdapterType
    {
        None,
        AppLovin,
        IronSource,
    }

    [Flags]
    public enum EAdsType
    {
        Inter = 1 << 0,
        Banner = 1 << 1,
        Reward = 1 << 2,
        Inter_Reward = 1 << 3,
        MRec = 1 << 4
    }

    public enum EAdsBannerPosition
    {
        TopLeft,
        TopCenter,
        TopRight,
        Centered,
        CenterLeft,
        CenterRight,
        BottomLeft,
        BottomCenter,
        BottomRight
    }

    public class AdsInitConfig
    {
        public EAdsBannerPosition bannerPosition;
        public string userId;

        public AdsInitConfig(EAdsBannerPosition bannerPosition = EAdsBannerPosition.BottomCenter,
            string userId = null)
        {
            this.bannerPosition = bannerPosition;
            this.userId = userId;
        }
    }

    public class AdsShowData
    {
        public Action<bool> callback;
        public string location;
        public string placement;
        public string rewardType;

        public bool haveReward;

        public AdsShowData(Action<bool> callback = null, string location = null,
            string placement = null, string rewardType = null)
        {
            this.callback = callback;
            this.location = location;
            this.placement = placement;
            this.rewardType = rewardType;
        }
    }

    public class AdsRevenueData
    {
        public string adPlatform;
        public string adSource;
        public string adUnitName;
        public string adFormat;
        public double value;
        public string currency;
        public string placement;

        public AdsRevenueData(string adPlatform, string adSource, string adUnitName, 
            string adFormat, double value, string currency, string placement)
        {
            this.adPlatform = adPlatform;
            this.adSource = adSource;
            this.adUnitName = adUnitName;
            this.adFormat = adFormat;
            this.value = value;
            this.currency = currency;
            this.placement = placement;
        }
    }

    public interface IAdsCallbackListener
    {
        void OnAdsRevenuePaid(AdsRevenueData data);
        void OnRewardLoaded();
        void OnRewardClicked();
        void OnRewardDisplayed();
        void OnRewardDisplayFailed();
        void OnRewardRecieved();
        void OnInterLoadFailed();
        void OnInterLoaded();
        void OnInterDisplayed();
        void OnInterClicked();
    }
}


