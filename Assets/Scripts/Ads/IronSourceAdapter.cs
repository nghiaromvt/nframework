using UnityEngine;

namespace NFramework.Ads
{
    public class IronSourceAdapter : AdsAdapterBase
    {
        [SerializeField] private string _appKeyAndroid;
        [SerializeField] private string _appKeyIOS;
        [Header("Debug")]
        [SerializeField] private bool _isEnableTestSuite;
        [SerializeField] private bool _useTestAds;

        public override EAdsAdapterType AdapterType => EAdsAdapterType.IronSource;

        public string AppKey
        {
            get
            {
                if (_useTestAds)
                    return DeviceInfo.IsAndroid ? "85460dcd" : "8545d445";
                else
                    return DeviceInfo.IsAndroid ? _appKeyAndroid : _appKeyIOS;
            }
        }

#if USE_IRONSOURCE_ADS
        protected override void OnApplicationPause(bool isPaused)
        {
            base.OnApplicationPause(isPaused);
            IronSource.Agent.onApplicationPause(isPaused);
        }

        public override void Init(AdsAdapterConfig config)
        {
            base.Init(config);

            if (Application.isEditor)
            {
                IsInitialized = true;
            }
            else
            {
                IronSource.Agent.setMetaData("is_child_directed", "false");
                IronSource.Agent.shouldTrackNetworkState(true);

                if (AdsManager.I.ConsentStatus == EConsentStatus.Unknown)
                {
                    Debug.LogError($"IronSource cannot init due to ConsentStatus = Unknown", this);
                    return;
                }
                else
                {
                    SetConsent(AdsManager.I.ConsentStatus);
                }

                AdsManager.OnConsentStatusChanged += SetConsent;
                IronSourceEvents.onSdkInitializationCompletedEvent += OnSDKInitialized;
                IronSource.Agent.init(AppKey);

                if (DeviceInfo.IsDevelopment && _isEnableTestSuite)
                    IronSource.Agent.setMetaData("is_test_suite", "enable");

#if USE_IRONSOURCE_AD_QUALITY
                IronSourceAdQuality.Initialize(AppKey);
#endif
            }
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

            IronSourceEvents.onImpressionDataReadyEvent += data => HandleAdsRevenuePaid(new AdsRevenueData("ironSource", data.adNetwork,
                  data.instanceName, data.adUnit, data.revenue.GetValueOrDefault(), "USD", data.placement));
        }

        private void SetConsent(EConsentStatus status) => IronSource.Agent.setConsent(AdsManager.I.ConsentStatus == EConsentStatus.Yes);

        #region Inter
        private void InitializeInter()
        {
            IronSourceInterstitialEvents.onAdReadyEvent += info => HandleInterLoaded();
            IronSourceInterstitialEvents.onAdLoadFailedEvent += error => HandleInterLoadFailed(error.ToString());
            IronSourceInterstitialEvents.onAdOpenedEvent += info => HandleInterDisplayed();
            IronSourceInterstitialEvents.onAdShowFailedEvent += (error, info) => HandleInterDisplayFailed(error.ToString());
            IronSourceInterstitialEvents.onAdClosedEvent += info => HandleInterHidden();
            IronSourceInterstitialEvents.onAdClickedEvent += info => HandleInterClicked();
            LoadInter();
        }

        protected override bool IsInterReadySDK() => Application.isEditor ? true : IronSource.Agent.isInterstitialReady();

        protected override void LoadInterSDK()
        {
            base.LoadInterSDK();
            IronSource.Agent.loadInterstitial();
        }

        protected override void ShowInterSDK()
        {
            base.ShowInterSDK();
            if (Application.isEditor)
            {
                HandleInterDisplayed();
                HandleInterHidden();
            }
            else
            {
                IronSource.Agent.showInterstitial();
            }
        }
        #endregion

        #region Reward
        private void InitializeReward()
        {
            IronSourceRewardedVideoEvents.onAdOpenedEvent += info => HandleRewardDisplayed();
            IronSourceRewardedVideoEvents.onAdShowFailedEvent += (error, info) => HandleRewardDisplayFailed(error.ToString());
            IronSourceRewardedVideoEvents.onAdRewardedEvent += (placement, info) => HandleRewardRecieved();
            IronSourceRewardedVideoEvents.onAdClosedEvent += info => HandleRewardHidden();
            IronSourceRewardedVideoEvents.onAdAvailableEvent += info => HandleRewardAvailable();
            IronSourceRewardedVideoEvents.onAdLoadFailedEvent += error => HandleRewardLoadFailed(error.ToString());
            IronSourceRewardedVideoEvents.onAdClickedEvent += (placement, info) => HandleRewardClicked();
        }

        protected override bool IsRewardReadySDK() => Application.isEditor ? true : IronSource.Agent.isRewardedVideoAvailable();

        protected override void LoadRewardSDK()
        {
            base.LoadRewardSDK();
            Logger.Log("Reward is load automatically, no need to call", this);
        }

        protected override void ShowRewardSDK()
        {
            base.ShowRewardSDK();
            if (Application.isEditor)
            {
                HandleRewardDisplayed();
                HandleRewardRecieved();
                HandleRewardHidden();
            }
            else
            {
                IronSource.Agent.showRewardedVideo();
            }
        }
        #endregion

        #region Banner
        private void InitializeBanner()
        {
            IronSourceBannerEvents.onAdLoadFailedEvent += error => HandleBannerLoadFailed(error.ToString());
            LoadBannerSDK();
        }

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
