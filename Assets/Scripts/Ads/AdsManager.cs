using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NFramework.Ads
{
    public class AdsManager : SingletonMono<AdsManager>, ISaveable
    {
        public static event Action<bool> OnIsRemoveAdsChanged;
        public static event Action<EConsentStatus> OnConsentStatusChanged;

        [SerializeField] private SaveData _saveData;
        [SerializeField] private EAdsBannerPosition _defaultBannerPosition = EAdsBannerPosition.BottomCenter;

        public Dictionary<EAdsAdapterType, AdsAdapterBase> AdapterDic { get; private set; } = new Dictionary<EAdsAdapterType, AdsAdapterBase>();
        public bool IsAllAdapterInitialized => !AdapterDic.Values.Any(x => !x.IsInitialized);
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

                    Logger.Log($"IsRemoveAds:{IsRemoveAds}");
                    OnIsRemoveAdsChanged?.Invoke(value);
                }
            }
        }

        public EConsentStatus ConsentStatus
        {
            get => _saveData.consentStatus;
            set
            {
                if (_saveData.consentStatus != value)
                {
                    Logger.Log($"ConsentStatus:{value}");
                    _saveData.consentStatus = value;
                    DataChanged = true;
                    OnConsentStatusChanged?.Invoke(value);
                    SaveManager.I.Save();
                }
            }
        }

        protected override void Awake()
        {
            base.Awake();
            foreach (var adapter in GetComponentsInChildren<AdsAdapterBase>())
                AdapterDic.Add(adapter.AdapterType, adapter);
        }

        public void Init(EAdsAdapterType specificAdapterType = EAdsAdapterType.None, IAdsCallbackListener adsCallbackListener = null)
        {
            if (DeviceInfo.IsNoAds)
                return;

            var config = new AdsAdapterConfig { defaultBannerPosition = _defaultBannerPosition, adsCallbackListener = adsCallbackListener };
            if (specificAdapterType == EAdsAdapterType.None)
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

        #region Inter
        public bool IsInterReady(EAdsAdapterType specificAdapterType = EAdsAdapterType.None)
        {
            if (IsRemoveAds || DeviceInfo.IsNoAds)
                return false;

            if (specificAdapterType == EAdsAdapterType.None)
            {
                foreach (var adapter in AdapterDic.Values)
                {
                    if (adapter.AdsTypeUse.HasFlag(EAdsType.Inter))
                        return adapter.IsInterReady();
                }
            }
            else
            {
                if (TryGetAdapter(specificAdapterType, out var adapter))
                    return adapter.IsInterReady();
            }

            return false;
        }

        public void LoadInter(EAdsAdapterType specificAdapterType = EAdsAdapterType.None)
        {
            if (IsRemoveAds || DeviceInfo.IsNoAds)
                return;

            if (specificAdapterType == EAdsAdapterType.None)
            {
                foreach (var adapter in AdapterDic.Values)
                {
                    if (adapter.AdsTypeUse.HasFlag(EAdsType.Inter))
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

            if (IsFullscreenAdShowing)
            {
                data?.callback?.Invoke(false);
                return;
            }    

            if (TryGetAdapter(specificAdapterType, out var adapter))
                adapter.ShowInter(data);
            else
                data?.callback?.Invoke(false);
        }
        #endregion

        #region Reward
        public bool IsRewardReady(EAdsAdapterType specificAdapterType = EAdsAdapterType.None)
        {
            if (IsRemoveAds || DeviceInfo.IsNoAds)
                return false;

            if (specificAdapterType == EAdsAdapterType.None)
            {
                foreach (var adapter in AdapterDic.Values)
                {
                    if (adapter.AdsTypeUse.HasFlag(EAdsType.Reward))
                        return adapter.IsRewardReady();
                }
            }
            else
            {
                if (TryGetAdapter(specificAdapterType, out var adapter))
                    return adapter.IsRewardReady();
            }

            return false;
        }

        public void LoadReward(EAdsAdapterType specificAdapterType = EAdsAdapterType.None)
        {
            if (IsRemoveAds || DeviceInfo.IsNoAds)
                return;

            if (specificAdapterType == EAdsAdapterType.None)
            {
                foreach (var adapter in AdapterDic.Values)
                {
                    if (adapter.AdsTypeUse.HasFlag(EAdsType.Reward))
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

            if (IsFullscreenAdShowing)
            {
                data?.callback?.Invoke(false);
                return;
            }

            if (TryGetAdapter(specificAdapterType, out var adapter))
                adapter.ShowReward(data);
            else
                data?.callback?.Invoke(false);
        }
        #endregion

        #region
        public void LoadBanner(EAdsAdapterType specificAdapterType = EAdsAdapterType.None)
        {
            if (IsRemoveAds || DeviceInfo.IsNoAds)
                return;

            if (specificAdapterType == EAdsAdapterType.None)
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

        public void ShowBanner(EAdsAdapterType specificAdapterType = EAdsAdapterType.None)
        {
            if (IsRemoveAds || DeviceInfo.IsNoAds)
                return;

            if (specificAdapterType == EAdsAdapterType.None)
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

        public void HideBanner(EAdsAdapterType specificAdapterType = EAdsAdapterType.None)
        {
            if (specificAdapterType == EAdsAdapterType.None)
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
        #endregion

        #region AOA
        public bool IsAOAReady(EAdsAdapterType specificAdapterType = EAdsAdapterType.None)
        {
            if (IsRemoveAds || DeviceInfo.IsNoAds)
                return false;

            if (specificAdapterType == EAdsAdapterType.None)
            {
                foreach (var adapter in AdapterDic.Values)
                {
                    if (adapter.AdsTypeUse.HasFlag(EAdsType.AOA))
                        return adapter.IsAOAReady();
                }
            }
            else
            {
                if (TryGetAdapter(specificAdapterType, out var adapter))
                    return adapter.IsAOAReady();
            }

            return false;
        }

        public void LoadAOA(EAdsAdapterType specificAdapterType = EAdsAdapterType.None)
        {
            if (IsRemoveAds || DeviceInfo.IsNoAds)
                return;

            if (specificAdapterType == EAdsAdapterType.None)
            {
                foreach (var adapter in AdapterDic.Values)
                {
                    if (adapter.AdsTypeUse.HasFlag(EAdsType.AOA))
                        adapter.LoadAOA();
                }
            }
            else
            {
                if (TryGetAdapter(specificAdapterType, out var adapter))
                    adapter.LoadAOA();
            }
        }

        public void ShowAOA(EAdsAdapterType specificAdapterType = EAdsAdapterType.None)
        {
            if (IsRemoveAds || DeviceInfo.IsNoAds || IsFullscreenAdShowing)
                return;

            if (specificAdapterType == EAdsAdapterType.None)
            {
                foreach (var adapter in AdapterDic.Values)
                {
                    if (adapter.AdsTypeUse.HasFlag(EAdsType.AOA))
                        adapter.ShowAOA();
                }
            }
            else
            {
                if (TryGetAdapter(specificAdapterType, out var adapter))
                    adapter.ShowAOA();
            }
        }
        #endregion

        public bool TryGetAdapter(EAdsAdapterType specificAdapterType, out AdsAdapterBase adapter)
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
            public EConsentStatus consentStatus;
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
        AdMob,
    }

    [Flags]
    public enum EAdsType
    {
        Inter = 1 << 0,
        Banner = 1 << 1,
        Reward = 1 << 2,
        Inter_Reward = 1 << 3,
        MRec = 1 << 4,
        AOA = 1 << 5,
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

    public enum EConsentStatus
    {
        Unknown,
        Yes,
        No
    }

    public class AdsAdapterConfig
    {
        public EAdsBannerPosition defaultBannerPosition;
        public IAdsCallbackListener adsCallbackListener;

        public AdsAdapterConfig(EAdsBannerPosition defaultBannerPosition = EAdsBannerPosition.BottomCenter,
            IAdsCallbackListener adsCallbackListener = null)
        {
            this.defaultBannerPosition = defaultBannerPosition;
            this.adsCallbackListener = adsCallbackListener;
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
        void OnRequestShowReward();
        void OnRewardLoaded();
        void OnRewardLoadFailed();
        void OnRewardClicked();
        void OnRewardDisplayed();
        void OnRewardDisplayFailed();
        void OnRewardRecieved();
        void OnRequestShowInter();
        void OnInterLoadFailed();
        void OnInterLoaded();
        void OnInterDisplayed();
        void OnInterClicked();
        void OnAOADisplayed();
    }
}


