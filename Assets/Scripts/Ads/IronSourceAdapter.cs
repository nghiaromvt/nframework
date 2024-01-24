using System;
using UnityEngine;

namespace NFramework.Ads
{
    public class IronSourceAdapter : AdsAdapterBase
    {
        [SerializeField] private string _appKeyAndroid;
        [SerializeField] private string _appKeyIOS;
        [Header("Debug")]
        [SerializeField] private bool _isEnableTestSuite;

        public override EAdsAdapterType AdapterType => EAdsAdapterType.IronSource;
        public string AppKey => DeviceInfo.IsAndroid ? _appKeyAndroid : _appKeyIOS;

#if USE_IRONSOURCE_ADS
        protected override void OnApplicationPause(bool isPaused)
        {
            base.OnApplicationPause(isPaused);
            IronSource.Agent.onApplicationPause(isPaused);
        }

        public override void Init(AdsInitConfig config, IAdsCallbackListener adsCallbackListener)
        {
            base.Init(config, adsCallbackListener);

            //IronSource.Agent.setMetaData("is_child_directed", "false");
            //IronSource.Agent.setConsent(consentValue);
            IronSource.Agent.shouldTrackNetworkState(true);

            IronSourceEvents.onSdkInitializationCompletedEvent += OnSDKInitialized;
            IronSource.Agent.init(AppKey);

            if (DeviceInfo.IsDevelopment && _isEnableTestSuite)
                IronSource.Agent.setMetaData("is_test_suite", "enable");

#if USE_IRONSOURCE_AD_QUALITY
            IronSourceAdQuality.Initialize(AppKey);
#endif
        }

        private void OnSDKInitialized()
        {
            Logger.Log($"OnSdkInitialized appKey:{AppKey}", this);
            IsInitialized = true;

            if (DeviceInfo.IsDevelopment && _isEnableTestSuite)
                IronSource.Agent.launchTestSuite();

            if (AdsTypeUse.HasFlag(EAdsType.Banner))
                InitializeBanner();

            if (AdsTypeUse.HasFlag(EAdsType.Inter))
                InitializeInter();

            if (AdsTypeUse.HasFlag(EAdsType.Inter))
                InitializeReward();

            IronSourceEvents.onImpressionDataReadyEvent += ImpressionDataReadyEvent;
        }

        private void ImpressionDataReadyEvent(IronSourceImpressionData data)
        {
            if (data != null)
            {
                _adsCallbackListener?.OnAdsRevenuePaid(new AdsRevenueData("ironSource", data.adNetwork,
                  data.instanceName, data.adUnit, data.revenue.GetValueOrDefault(), "USD", data.placement));
            }
        }

        #region Inter
        private void InitializeInter()
        {
            IronSourceInterstitialEvents.onAdReadyEvent += OnInterLoaded;
            IronSourceInterstitialEvents.onAdLoadFailedEvent += OnInterLoadFailed;
            IronSourceInterstitialEvents.onAdOpenedEvent += OnInterDisplayed;
            IronSourceInterstitialEvents.onAdShowFailedEvent += OnInterDisplayFailed;
            IronSourceInterstitialEvents.onAdClosedEvent += OnInterHidden;
            IronSourceInterstitialEvents.onAdClickedEvent += OnInterClicked;
            LoadInter();
        }

        private void OnInterClicked(IronSourceAdInfo info) => _adsCallbackListener?.OnInterClicked();

        private void OnInterLoaded(IronSourceAdInfo info)
        {
            _interLoadRetryAttempt = 0;
            _adsCallbackListener?.OnInterLoaded();
        }

        private void OnInterLoadFailed(IronSourceError error)
        {
            Debug.LogError($"OnInterLoadFailed error:{error}", this);
            _interLoadRetryAttempt++;
            var retryDelay = Mathf.Pow(2, Mathf.Min(6, _interLoadRetryAttempt));
            this.InvokeDelayRealtime(retryDelay, LoadInter);
            _adsCallbackListener?.OnInterLoadFailed();
        }

        private void OnInterDisplayed(IronSourceAdInfo info)
        {
            IsFullscreenAdShowing = true;
            _adsCallbackListener?.OnInterDisplayed();
        }

        private void OnInterDisplayFailed(IronSourceError error, IronSourceAdInfo info)
        {
            IsFullscreenAdShowing = false;
            if (_cachedAdsShowDataDic.TryGetValue(EAdsType.Inter, out var data) && data != null)
            {
                data.callback?.Invoke(false);
                _cachedAdsShowDataDic.Remove(EAdsType.Inter);
            }
            LoadInter();
        }

        private void OnInterHidden(IronSourceAdInfo info)
        {
            IsFullscreenAdShowing = false;
            if (_cachedAdsShowDataDic.TryGetValue(EAdsType.Inter, out var data) && data != null)
            {
                data.callback?.Invoke(true);
                _cachedAdsShowDataDic.Remove(EAdsType.Inter);
            }
            LoadInter();
        }

        protected override bool IsInterReadySDK() => IronSource.Agent.isInterstitialReady();

        protected override void LoadInterSDK()
        {
            base.LoadInterSDK();
            IronSource.Agent.loadInterstitial();
        }

        protected override void ShowInterSDK()
        {
            base.ShowInterSDK();
            IronSource.Agent.showInterstitial();
        }
        #endregion

        #region Reward
        private void InitializeReward()
        {
            IronSourceRewardedVideoEvents.onAdOpenedEvent += OnRewardDisplayed;
            IronSourceRewardedVideoEvents.onAdShowFailedEvent += OnRewardDisplayFailed;
            IronSourceRewardedVideoEvents.onAdRewardedEvent += OnRewardRecieved;
            IronSourceRewardedVideoEvents.onAdClosedEvent += OnRewardHidden;
            IronSourceRewardedVideoEvents.onAdAvailableEvent += OnRewardAvailable;
            IronSourceRewardedVideoEvents.onAdUnavailableEvent += OnRewardUnavailable;
            IronSourceRewardedVideoEvents.onAdLoadFailedEvent += OnRewardLoadFailed;
            IronSourceRewardedVideoEvents.onAdClickedEvent += OnRewardClicked;
        }

        private void OnRewardClicked(IronSourcePlacement placement, IronSourceAdInfo info) => _adsCallbackListener?.OnRewardClicked();

        private void OnRewardLoadFailed(IronSourceError error) => Debug.LogError($"OnRewardLoadFailed error:{error}", this);

        private void OnRewardDisplayed(IronSourceAdInfo info)
        {
            IsFullscreenAdShowing = true;
            _adsCallbackListener?.OnRewardDisplayed();
        }

        private void OnRewardDisplayFailed(IronSourceError error, IronSourceAdInfo info)
        {
            Debug.LogError($"OnRewardDisplayFailed error:{error} info:{info}", this);
            IsFullscreenAdShowing = false;
            if (_cachedAdsShowDataDic.TryGetValue(EAdsType.Reward, out var data) && data != null)
            {
                data.callback?.Invoke(false);
                _cachedAdsShowDataDic.Remove(EAdsType.Reward);
            }
            _adsCallbackListener?.OnRewardDisplayFailed();
        }

        private void OnRewardRecieved(IronSourcePlacement placement, IronSourceAdInfo info)
        {
            if (_cachedAdsShowDataDic.TryGetValue(EAdsType.Reward, out var data) && data != null)
                data.haveReward = true;

            _adsCallbackListener.OnRewardRecieved();
        }

        private void OnRewardHidden(IronSourceAdInfo info)
        {
            IsFullscreenAdShowing = false;
            if (_cachedAdsShowDataDic.TryGetValue(EAdsType.Reward, out var data) && data != null)
            {
                data.callback?.Invoke(data.haveReward);
                _cachedAdsShowDataDic.Remove(EAdsType.Reward);
            }
        }

        private void OnRewardAvailable(IronSourceAdInfo info)
        {
            Logger.Log($"IsRewardReady:{IsRewardReady()}", this);
            _adsCallbackListener?.OnRewardLoaded();
        }

        private void OnRewardUnavailable() => Logger.Log($"IsRewardReady:{IsRewardReady()}", this);

        protected override bool IsRewardReadySDK() => IronSource.Agent.isRewardedVideoAvailable();

        protected override void LoadRewardSDK()
        {
            base.LoadRewardSDK();
            Logger.Log("Reward is load automatically, no need to call", this);
        }

        protected override void ShowRewardSDK()
        {
            base.ShowRewardSDK();
            IronSource.Agent.showRewardedVideo();
        }
        #endregion

        #region Banner
        private void InitializeBanner()
        {
            IronSourceBannerEvents.onAdLoadFailedEvent += OnBannerLoadFailed;
            LoadBannerSDK();
        }

        private void OnBannerLoadFailed(IronSourceError error) => Logger.LogError($"OnBannerLoadFailed error:{error}", this);

        protected override void LoadBannerSDK()
        {
            base.LoadBannerSDK();
            IronSource.Agent.loadBanner(IronSourceBannerSize.BANNER, GetBannerPosition());
            HideBanner();
        }

        protected override void ShowBannerSDK()
        {
            base.ShowBannerSDK();
            IronSource.Agent.displayBanner();
        }

        public override void HideBanner()
        {
            base.HideBanner();
            IronSource.Agent.hideBanner();
        }

        public override void DestroyBanner()
        {
            base.DestroyBanner();
            IronSource.Agent.destroyBanner();
        }

        private IronSourceBannerPosition GetBannerPosition()
        {
            switch (BannerPosition)
            {
                case EAdsBannerPosition.TopCenter:
                    return IronSourceBannerPosition.TOP;
                case EAdsBannerPosition.BottomCenter:
                    return IronSourceBannerPosition.BOTTOM;
                default:
                    Logger.LogError($"{BannerPosition} is not supported", this);
                    return IronSourceBannerPosition.BOTTOM;
            }
        }
        #endregion
#endif
    }
}
