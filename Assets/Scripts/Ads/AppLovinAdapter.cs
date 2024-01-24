using System;
using UnityEngine;

namespace NFramework.Ads
{
    // Warning: Outdated
    public class AppLovinAdapter : AdsAdapterBase
    {
        [SerializeField] private string _sdkKey;
        [Header("Android")]
        [SerializeField, ConditionalField(nameof(_adsTypeUse), compareValues: EAdsType.Banner)]
        private string _bannerAdUnitIdAndroid;
        [SerializeField, ConditionalField(nameof(_adsTypeUse), compareValues: EAdsType.Inter)]
        private string _interAdUnitIdAndroid;
        [SerializeField, ConditionalField(nameof(_adsTypeUse), compareValues: EAdsType.Reward)]
        private string _rewardAdUnitIdAndroid;
        [Header("IOS")]
        [SerializeField, ConditionalField(nameof(_adsTypeUse), compareValues: EAdsType.Banner)]
        private string _bannerAdUnitIdIOS;
        [SerializeField, ConditionalField(nameof(_adsTypeUse), compareValues: EAdsType.Inter)]
        private string _interAdUnitIdIOS;
        [SerializeField, ConditionalField(nameof(_adsTypeUse), compareValues: EAdsType.Reward)]
        private string _rewardAdUnitIdIOS;
        [Header("Debug")]
        [SerializeField] private bool _isShowMediationDebugger;
        [SerializeField] private bool _isShowCreativeDebugger;

        private int _interRetryAttempt;
        private int _rewardRetryAttempt;
        private Action<bool> _showInterlCallback;
        private Action<bool> _showRewardCallback;
        private bool _receivedReward;

        public override EAdsAdapterType AdapterType => EAdsAdapterType.AppLovin;
        public string BannerAdUnitId => DeviceInfo.IsAndroid ? _bannerAdUnitIdAndroid : _bannerAdUnitIdIOS;
        public string InterAdUnitId => DeviceInfo.IsAndroid ? _interAdUnitIdAndroid : _interAdUnitIdIOS;
        public string RewardAdUnitId => DeviceInfo.IsAndroid ? _rewardAdUnitIdAndroid : _rewardAdUnitIdIOS;

//#if USE_APPLOVIN_ADS
//        public override void Init(InitAdsAdapterData data)
//        {
//            base.Init(data);

//            MaxSdkCallbacks.OnSdkInitializedEvent += (MaxSdkBase.SdkConfiguration sdkConfiguration) =>
//            {
//                Logger.Log($"OnSdkInitialized sdkKey:{_sdkKey}", this);
//                IsInitialized = true;

//                if (_adsTypeUse.HasFlag(AdsType.Banner))
//                    InitializeBanner(GetBannerPosition());

//                if (_adsTypeUse.HasFlag(AdsType.Inter))
//                    InitializeInter();

//                if (_adsTypeUse.HasFlag(AdsType.Reward))
//                    InitializeReward();

//                if (!Application.isEditor)
//                {
//                    if (_isShowMediationDebugger)
//                    {
//                        MaxSdk.ShowMediationDebugger();
//                        MaxSdk.SetVerboseLogging(true);
//                    }

//                    if (_isShowCreativeDebugger)
//                    {
//                        MaxSdk.SetCreativeDebuggerEnabled(true);
//                        MaxSdk.ShowCreativeDebugger();
//                    }
//                }
//            };

//            if (!string.IsNullOrEmpty(data.userId))
//                MaxSdk.SetUserId(data.userId);

//            MaxSdk.SetSdkKey(_sdkKey);
//            MaxSdk.InitializeSdk();
//        }

//        #region Inter
//        private void InitializeInter()
//        {
//            MaxSdkCallbacks.Interstitial.OnAdLoadedEvent += OnInterLoaded;
//            MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent += OnInterLoadFailed;
//            MaxSdkCallbacks.Interstitial.OnAdDisplayedEvent += OnInterDisplayed;
//            MaxSdkCallbacks.Interstitial.OnAdHiddenEvent += OnInterHidden;
//            MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent += OnInterFailedToDisplay;
//            MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent += OnAdRevenuePaid;
//            LoadInter();
//        }

//        private void LoadInter() => MaxSdk.LoadInterstitial(InterAdUnitId);

//        private void OnInterLoaded(string adUnitId, MaxSdkBase.AdInfo adInfo) => _interRetryAttempt = 0;

//        private void OnInterLoadFailed(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
//        {
//            Logger.LogError($"OnInterLoadFailed adUnitId:{adUnitId} errorInfo:{errorInfo}", this);
//            // AppLovin recommends that you retry with exponentially higher delays, up to a maximum delay (in this case 64 seconds)
//            _interRetryAttempt++;
//            var retryDelay = Mathf.Pow(2, Mathf.Min(6, _interRetryAttempt));
//            this.InvokeDelayRealtime(retryDelay, LoadInter);
//        }

//        private void OnInterDisplayed(string adUnitId, MaxSdkBase.AdInfo adInfo) => IsFullscreenAdShowing = true;

//        private void OnInterFailedToDisplay(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
//        {
//            IsFullscreenAdShowing = false;
//            _showInterlCallback?.Invoke(false);
//            _showInterlCallback = null;
//            LoadInter();
//        }

//        private void OnInterHidden(string adUnitId, MaxSdkBase.AdInfo adInfo)
//        {
//            IsFullscreenAdShowing = false;
//            _showInterlCallback?.Invoke(true);
//            _showInterlCallback = null;
//            LoadInter();
//        }

//        public override bool IsInterReady() => MaxSdk.IsInterstitialReady(InterAdUnitId);

//        protected override void _ShowInter(Action<bool> callback = null, RequestShowAdsData data = null)
//        {
//            base._ShowInter(callback, data);
//            _showInterlCallback = callback;
//            MaxSdk.ShowInterstitial(InterAdUnitId, data.placement);
//        }
//        #endregion

//        #region Reward
//        private void InitializeReward()
//        {
//            MaxSdkCallbacks.Rewarded.OnAdLoadedEvent += OnRewardLoaded;
//            MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent += OnRewardLoadFailed;
//            MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent += OnRewardAdDisplayed;
//            MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += OnRewardHidden;
//            MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent += OnRewardFailedToDisplay;
//            MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += OnReceivedReward;
//            MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += OnAdRevenuePaid;
//            LoadReward();
//        }

//        public void LoadReward() => MaxSdk.LoadRewardedAd(RewardAdUnitId);

//        private void OnRewardLoaded(string adUnitId, MaxSdkBase.AdInfo adInfo) => _rewardRetryAttempt = 0;

//        private void OnRewardLoadFailed(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
//        {
//            Logger.LogError($"OnRewardLoadFailed adUnitId:{adUnitId} errorInfo:{errorInfo}", this);
//            // AppLovin recommends that you retry with exponentially higher delays, up to a maximum delay (in this case 64 seconds).
//            _rewardRetryAttempt++;
//            var retryDelay = Mathf.Pow(2, Mathf.Min(6, _rewardRetryAttempt));
//            this.InvokeDelayRealtime(retryDelay, LoadReward);
//        }

//        private void OnRewardAdDisplayed(string adUnitId, MaxSdkBase.AdInfo adInfo) => IsFullscreenAdShowing = true;

//        private void OnRewardFailedToDisplay(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
//        {
//            Logger.LogError($"OnRewardFailedToDisplay adUnitId:{adUnitId} errorInfo:{errorInfo} adInfo:{adInfo}", this);
//            IsFullscreenAdShowing = false;
//            _showRewardCallback?.Invoke(false);
//            _showRewardCallback = null;
//            LoadReward();
//        }

//        private void OnRewardHidden(string adUnitId, MaxSdkBase.AdInfo adInfo)
//        {
//            IsFullscreenAdShowing = false;
//            _showRewardCallback?.Invoke(_receivedReward);
//            _showRewardCallback = null;
//            LoadReward();
//        }

//        private void OnReceivedReward(string adUnitId, MaxSdk.Reward reward, MaxSdkBase.AdInfo adInfo) => _receivedReward = true;

//        public bool IsRewardedAdReady() => MaxSdk.IsRewardedAdReady(RewardAdUnitId);

//        protected override void _ShowReward(Action<bool> callback = null, RequestShowAdsData data = null)
//        {
//            base._ShowReward(callback, data);
//            _receivedReward = false;
//            _showRewardCallback = callback;
//            MaxSdk.ShowRewardedAd(RewardAdUnitId, data.placement);
//        }
//        #endregion

//        #region Banner
//        private void InitializeBanner(MaxSdkBase.BannerPosition bannerPosition)
//        {
//            MaxSdk.CreateBanner(BannerAdUnitId, bannerPosition);
//            MaxSdk.SetBannerBackgroundColor(BannerAdUnitId, Color.clear);
//            MaxSdkCallbacks.Banner.OnAdLoadFailedEvent += OnBannerLoadFailed;
//            MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent += OnAdRevenuePaid;
//        }

//        private void OnBannerLoadFailed(string adUnitId, MaxSdkBase.ErrorInfo errorInfo) =>
//            Logger.LogError($"OnBannerLoadFailed adUnitId:{adUnitId} errorInfo:{errorInfo}", this);

//        protected override void _ShowBanner()
//        {
//            base.ShowBanner();
//            MaxSdk.ShowBanner(BannerAdUnitId);
//        }

//        public override void HideBanner()
//        {
//            base.HideBanner();
//            MaxSdk.HideBanner(BannerAdUnitId);
//        }

//        public override void DestroyBanner()
//        {
//            base.DestroyBanner();
//            MaxSdk.DestroyBanner(BannerAdUnitId);
//        }

//        private MaxSdkBase.BannerPosition GetBannerPosition()
//        {
//            switch (BannerPosition)
//            {
//                case EAdsBannerPosition.TopLeft: return MaxSdkBase.BannerPosition.TopLeft;
//                case EAdsBannerPosition.TopCenter: return MaxSdkBase.BannerPosition.TopCenter;
//                case EAdsBannerPosition.TopRight: return MaxSdkBase.BannerPosition.TopRight;
//                case EAdsBannerPosition.Centered: return MaxSdkBase.BannerPosition.Centered;
//                case EAdsBannerPosition.CenterLeft: return MaxSdkBase.BannerPosition.CenterLeft;
//                case EAdsBannerPosition.CenterRight: return MaxSdkBase.BannerPosition.CenterRight;
//                case EAdsBannerPosition.BottomLeft: return MaxSdkBase.BannerPosition.BottomLeft;
//                default:
//                case EAdsBannerPosition.BottomCenter: return MaxSdkBase.BannerPosition.BottomCenter;
//                case EAdsBannerPosition.BottomRight: return MaxSdkBase.BannerPosition.BottomRight;
//            }
//        }

//        public Rect GetBannerRect()
//        {
//            var absoluteRect = MaxSdk.GetBannerLayout(BannerAdUnitId);
//            var density = GetScreenDensity();
//            var unityRect = absoluteRect;
//            unityRect.x *= density;
//            unityRect.y *= density;
//            unityRect.width *= density;
//            unityRect.height *= density;
//            return unityRect;
//        }

//        public void SetBannerWidth(float unityWidth)
//        {
//            // Be sure to set the banner width to a size larger than the minimum value (320 on phones, 728 on tablets)
//            var width = unityWidth / GetScreenDensity();
//            MaxSdk.SetBannerWidth(BannerAdUnitId, width);
//        }

//        public float GetAdaptiveBannerHeight(float unityWidth = -1)
//        {
//            var density = GetScreenDensity();
//            var absoluteHeight = MaxSdkUtils.GetAdaptiveBannerHeight(unityWidth / density);
//            return absoluteHeight * density;
//        }

//        public float GetScreenDensity() => MaxSdkUtils.GetScreenDensity();
//        #endregion

//        private void OnAdRevenuePaid(string adUnitId, MaxSdkBase.AdInfo impressionData)
//        {
//            //AdsManager.I.HandleOnAdRevenuePaidEvent("AppLovin",
//            //    impressionData.NetworkName, impressionData.AdUnitIdentifier,
//            //    impressionData.AdFormat, impressionData.Revenue, "USD", impressionData.Placement);
//        }
//#endif
    }
}
