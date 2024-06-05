#if USE_ADMOB_ADS
using GoogleMobileAds.Api;
#endif
using System;
using UnityEngine;

namespace NFramework.Ads
{
    public class AdMobAdapter : AdsAdapterBase
    {
        [Header("Android")]
        [SerializeField, ConditionalField(nameof(_adsTypeUse), compareValues: EAdsType.AOA)]
        private string _aoaUnitIdAndroid;
        [SerializeField, ConditionalField(nameof(_adsTypeUse), compareValues: EAdsType.Banner)]
        private string _bannerUnitIdAndroid;
        [SerializeField, ConditionalField(nameof(_adsTypeUse), compareValues: EAdsType.Native)]
        private string _nativeUnitIdAndroid;
        [Header("IOS")]
        [SerializeField, ConditionalField(nameof(_adsTypeUse), compareValues: EAdsType.AOA)]
        private string _aoaUnitIdIOS;
        [SerializeField, ConditionalField(nameof(_adsTypeUse), compareValues: EAdsType.Banner)]
        private string _bannerUnitIdIOS;
        [SerializeField, ConditionalField(nameof(_adsTypeUse), compareValues: EAdsType.Native)]
        private string _nativeUnitIdIOS;
        [Header("Debug")]
        [SerializeField] private bool _useTestAds;

        private DateTime _aoaExpireTime;
        private DateTime _nativeAdExpireTime;

        public override EAdsAdapterType AdapterType => EAdsAdapterType.AdMob;

        public string AOAUnitId
        {
            get
            {
                if (_useTestAds)
                    return DeviceInfo.IsAndroid ? "ca-app-pub-3940256099942544/9257395921" : "ca-app-pub-3940256099942544/5575463023";
                else
                    return DeviceInfo.IsAndroid ? _aoaUnitIdAndroid : _aoaUnitIdIOS;
            }
        }

        public string BannerUnitId
        {
            get
            {
                if (_useTestAds)
                    return DeviceInfo.IsAndroid ? "ca-app-pub-3940256099942544/2014213617" : "ca-app-pub-3940256099942544/8388050270";
                else
                    return DeviceInfo.IsAndroid ? _bannerUnitIdAndroid : _bannerUnitIdIOS;
            }
        }

        public string NativeUnitId
        {
            get
            {
                if (_useTestAds)
                    return DeviceInfo.IsAndroid ? "ca-app-pub-3940256099942544/2247696110" : "ca-app-pub-3940256099942544/2247696110";
                else
                    return DeviceInfo.IsAndroid ? _nativeUnitIdAndroid : _nativeUnitIdIOS;
            }
        }

#if USE_ADMOB_ADS
        private AppOpenAd _appOpenAd;
        private BannerView _bannerView;
#if USE_ADMOB_NATIVE_AD
        private NativeAd _nativeAd;
        private AdLoader _adLoader;
#endif

        public override void Init(AdsAdapterConfig config)
        {
            base.Init(config);

            if (Application.isEditor)
                IsInitialized = true;
            else
                MobileAds.Initialize(OnSDKInitialized);
        }

        private void OnSDKInitialized(InitializationStatus status)
        {
            Logger.Log($"OnSdkInitialized status:{status}", this);
            IsInitialized = true;
            if (_adsTypeUse.HasFlag(EAdsType.AOA))
                LoadAOA();

#if USE_ADMOB_NATIVE_AD
            if (_adsTypeUse.HasFlag(EAdsType.Native))
            {
                RequestNativeAd();
                LoadNativeAd();
            }
#endif
        }

        #region AOA
        protected override bool IsAOAReadySDK() => _appOpenAd != null && _appOpenAd.CanShowAd() && DateTime.Now < _aoaExpireTime;

        protected override void LoadAOASDK()
        {
            base.LoadAOASDK();

            if (_appOpenAd != null)
            {
                _appOpenAd.Destroy();
                _appOpenAd = null;
            }

            var adRequest = new AdRequest();
            AppOpenAd.Load(AOAUnitId, adRequest, (AppOpenAd ad, LoadAdError error) =>
            {
                if (error != null || ad == null)
                {
                    HandleAOALoadFailed(error.ToString());
                }
                else
                {
                    HandleAOALoaded();
                    _aoaExpireTime = DateTime.Now + TimeSpan.FromHours(4);
                    _appOpenAd = ad;
                    RegisterAOAEventHandlers(ad);
                }
            });
        }

        private void RegisterAOAEventHandlers(AppOpenAd ad)
        {
            ad.OnAdPaid += (AdValue adValue) => HandleAdsRevenuePaid(new AdsRevenueData("adMob", "",
                "", "", adValue.Value, adValue.CurrencyCode,
                "", adValue.Precision.ToString(), "", null, EAdsType.AOA));

            ad.OnAdFullScreenContentClosed += HandleAOAHidden;
            ad.OnAdFullScreenContentFailed += error => HandleAOADisplayFailed(error.ToString());
        }

        protected override void ShowAOASDK()
        {
            base.ShowAOASDK();
            _appOpenAd.Show();
        }
        #endregion

        #region Banner
        protected override void LoadBannerSDK()
        {
            base.LoadBannerSDK();
            // create an instance of a banner view first.
            if (_bannerView == null)
                CreateBannerView();

            // create our request used to load the ad.
            var adRequest = new AdRequest();
            adRequest.Extras.Add("collapsible", "bottom");
            adRequest.Extras.Add("collapsible_request_id", Guid.NewGuid().ToString());

            // send the request to load the ad.
            _bannerView?.LoadAd(adRequest);
        }

        protected override void ShowBannerSDK()
        {
            base.ShowBannerSDK();
            LoadBannerSDK();
        }

        public override void HideBanner()
        {
            base.HideBanner();
            DestroyBanner();
        }

        public override void DestroyBanner()
        {
            base.DestroyBanner();
            if (_bannerView != null)
            {
                _bannerView.Destroy();
                _bannerView = null;
            }
        }

        private void CreateBannerView()
        {
            // If we already have a banner, destroy the old one.
            if (_bannerView != null)
                DestroyBanner();

            AdSize adaptiveSize = AdSize.GetPortraitAnchoredAdaptiveBannerAdSizeWithWidth(AdSize.FullWidth);

            // Create a 320x50 banner at top of the screen
            _bannerView = new BannerView(BannerUnitId, adaptiveSize, AdPosition.Bottom);

            _bannerView.OnBannerAdLoaded += () => Debug.Log("Banner view loaded an ad with response : " + _bannerView.GetResponseInfo());
            _bannerView.OnBannerAdLoadFailed += error => HandleBannerLoadFailed(error.ToString());

            _bannerView.OnAdPaid += (AdValue adValue) => HandleAdsRevenuePaid(new AdsRevenueData("adMob", "",
                "", "", adValue.Value, adValue.CurrencyCode,
                "", adValue.Precision.ToString(), "", null, EAdsType.Banner));

            _bannerView.OnAdFullScreenContentOpened += () => IsFullscreenAdShowing = true;
            _bannerView.OnAdFullScreenContentClosed += () => DelayResetIsFullscreenAdShowing();
        }
        #endregion

#if USE_ADMOB_NATIVE_AD
        #region Native Ad
        private void RequestNativeAd()
        {
            _adLoader = new AdLoader.Builder(NativeUnitId).ForNativeAd().Build();
            _adLoader.OnNativeAdLoaded += OnNativeAdLoaded;
            _adLoader.OnAdFailedToLoad += OnNativeAdFailedToLoad;
            _adLoader.OnNativeAdOpening += OnNativeAdOpening;
            _adLoader.OnNativeAdClosed += OnNativeAdClosed;
        }

        private void OnNativeAdLoaded(object sender, NativeAdEventArgs args)
        {
            Logger.Log("Native ad loaded", this);
            _nativeAd = args.nativeAd;
            _nativeAd.OnPaidEvent += OnNativeAdPaid;
            _nativeAdExpireTime = DateTime.Now + TimeSpan.FromHours(1);
            HandleNativeAdLoaded();
        }

        private void OnNativeAdPaid(object sender, AdValueEventArgs args)
        {
            Logger.Log($"OnNativeAdPaid: value:{args.AdValue.Value} - currencyCode:{args.AdValue.CurrencyCode}", this);
            HandleAdsRevenuePaid(new AdsRevenueData("adMob", "",
                "", "", args.AdValue.Value, args.AdValue.CurrencyCode,
                "", args.AdValue.Precision.ToString(), "", null, EAdsType.AOA));
        }

        private void OnNativeAdFailedToLoad(object sender, AdFailedToLoadEventArgs args)
        {
            _nativeAd = null;
            HandleNativeAdLoadFailed(args.LoadAdError.GetMessage());
        }

        private void OnNativeAdClosed(object sender, EventArgs e) => DelayResetIsFullscreenAdShowing();

        private void OnNativeAdOpening(object sender, EventArgs e) => IsFullscreenAdShowing = true;

        protected override bool IsNativeAdReadySDK() => _nativeAd != null && DateTime.Now < _nativeAdExpireTime;

        protected override void LoadNativeAdSDK()
        {
            base.LoadNativeAdSDK();

            if (_adLoader == null)
                return;

            if (_nativeAd != null)
                _nativeAd.Destroy();

            _nativeAd = null;
            _adLoader.LoadAd(new AdRequest());
        }

        public bool TryGetNativeAd(out NativeAd nativeAd)
        {
            if (IsNativeAdReady())
            {
                nativeAd = _nativeAd;
                return true;
            }
            else
            {
                nativeAd = null;
                return false;
            }
        #endregion
#endif
#endif
    }
}

