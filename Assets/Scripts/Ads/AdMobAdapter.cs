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
        [Header("IOS")]
        [SerializeField, ConditionalField(nameof(_adsTypeUse), compareValues: EAdsType.AOA)]
        private string _aoaUnitIdIOS;
        [Header("Debug")]
        [SerializeField] private bool _useTestAds;

        private DateTime _aoaExpireTime;

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

#if USE_ADMOB_ADS
        public AppOpenAd _appOpenAd;

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
                    "", "", adValue.Value, adValue.CurrencyCode, ""));


            ad.OnAdFullScreenContentClosed += HandleAOAHidden;
            ad.OnAdFullScreenContentFailed += error => HandleAOADisplayFailed(error.ToString());
        }

        protected override void ShowAOASDK()
        {
            base.ShowAOASDK();
            _appOpenAd.Show();
        }
        #endregion
#endif
    }
}
