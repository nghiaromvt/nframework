using System;
using System.Collections.Generic;
using UnityEngine;

namespace NFramework.Ads
{
    public class AdsAdapterBase : MonoBehaviour
    {
        [SerializeField] protected EAdsType _adsTypeUse;

        protected Dictionary<EAdsType, AdsShowData> _cachedAdsShowDataDic = new Dictionary<EAdsType, AdsShowData>();
        protected int _interLoadRetryAttempt;
        protected int _rewardLoadRetryAttempt;
        protected int _aoaLoadRetryAttempt;
        protected AdsAdapterConfig _config;

        public virtual EAdsAdapterType AdapterType => EAdsAdapterType.None;
        public bool IsInitialized { get; protected set; }
        public EAdsBannerPosition BannerPosition { get; private set; }
        public bool IsFullscreenAdShowing { get; protected set; }
        public EAdsType AdsTypeUse => _adsTypeUse;

        protected virtual void OnApplicationPause(bool isPaused) { }

        #region Init
        public virtual void Init(AdsAdapterConfig config)
        {
            BannerPosition = config.defaultBannerPosition;
            _config = config;
        }
        #endregion

        #region Inter
        public bool IsInterReady()
        {
            if (!IsInitialized)
            {
                Logger.LogError($"Adapter didn't initialize", this);
                return false;
            }

            if (_adsTypeUse.HasFlag(EAdsType.Inter))
            {
                return IsInterReadySDK();
            }
            else
            {
                Logger.LogError($"Adapter doesn't handle Inter", this);
                return false;
            }
        }

        protected virtual bool IsInterReadySDK() => false;

        public void LoadInter()
        {
            if (!IsInitialized)
            {
                Logger.LogError($"Adapter didn't initialize", this);
                return;
            }

            if (_adsTypeUse.HasFlag(EAdsType.Inter))
                LoadInterSDK();
            else
                Logger.LogError($"Adapter doesn't handle Inter", this);
        }

        protected virtual void LoadInterSDK() { }

        public void ShowInter(AdsShowData data)
        {
            if (!IsInitialized)
            {
                Logger.LogError($"Adapter didn't initialize", this);
                data?.callback?.Invoke(false);
                return;
            }

            if (_adsTypeUse.HasFlag(EAdsType.Inter))
            {
                if (IsInterReady())
                {
                    _cachedAdsShowDataDic[EAdsType.Inter] = data;
                    ShowInterSDK();
                }
                else
                {
                    data?.callback?.Invoke(false);
                }
            }
            else
            {
                Logger.LogError($"Adapter doesn't handle Inter", this);
                data?.callback?.Invoke(false);
            }
        }

        protected virtual void ShowInterSDK() { }
        #endregion

        #region Reward
        public bool IsRewardReady()
        {
            if (!IsInitialized)
            {
                Logger.LogError($"Adapter didn't initialize", this);
                return false;
            }

            if (_adsTypeUse.HasFlag(EAdsType.Reward))
            {
                return IsRewardReadySDK();
            }
            else
            {
                Logger.LogError($"Adapter doesn't handle Reward", this);
                return false;
            }
        }

        protected virtual bool IsRewardReadySDK() => false;

        public void LoadReward()
        {
            if (!IsInitialized)
            {
                Logger.LogError($"Adapter didn't initialize", this);
                return;
            }

            if (_adsTypeUse.HasFlag(EAdsType.Reward))
                LoadRewardSDK();
            else
                Logger.LogError($"Adapter doesn't handle Reward", this);
        }

        protected virtual void LoadRewardSDK() { }

        public void ShowReward(AdsShowData data)
        {
            if (!IsInitialized)
            {
                Logger.LogError($"Adapter didn't initialize", this);
                data?.callback?.Invoke(false);
                return;
            }

            if (_adsTypeUse.HasFlag(EAdsType.Reward))
            {
                if (IsRewardReady())
                {
                    _cachedAdsShowDataDic[EAdsType.Reward] = data;
                    ShowRewardSDK();
                }
                else
                {
                    data?.callback?.Invoke(false);
                }
            }
            else
            {
                Logger.LogError($"Adapter doesn't handle Reward", this);
                data?.callback?.Invoke(false);
            }
        }

        protected virtual void ShowRewardSDK() { }

        protected void HandleRewardLoaded(string errorMessage = null) =>
            Logger.Log($"HandleRewardLoaded: {errorMessage}", this);

        protected void HandleRewardFailedToLoad() { }

        protected void HandleRewardDisplayed() => IsFullscreenAdShowing = true;

        protected void HandleRewardFailedToDisplay(string errorMessage = null)
        {
            Logger.Log($"HandleRewardFailedToDisplay: {errorMessage}", this);
            IsFullscreenAdShowing = false;
            _cachedAdsShowDataDic[EAdsType.Reward].callback?.Invoke(false);
        }

        protected void HandleReceivedReward() => _cachedAdsShowDataDic[EAdsType.Reward].haveReward = true;

        protected void HandleRewardHidden()
        {
            IsFullscreenAdShowing = false;
            var adsShowData = _cachedAdsShowDataDic[EAdsType.Reward];
            adsShowData.callback?.Invoke(adsShowData.haveReward);
        }

        protected void HandleRewardClicked() { }
        #endregion

        #region Banner
        public void LoadBanner()
        {
            if (!IsInitialized)
            {
                Logger.LogError($"Adapter didn't initialize", this);
                return;
            }

            if (_adsTypeUse.HasFlag(EAdsType.Banner))
                LoadBannerSDK();
            else
                Logger.LogError($"Adapter doesn't handle Banner", this);
        }

        protected virtual void LoadBannerSDK() { }

        public void ShowBanner()
        {
            if (!IsInitialized)
            {
                Logger.LogError($"Adapter didn't initialize", this);
                return;
            }

            if (_adsTypeUse.HasFlag(EAdsType.Banner))
                ShowBannerSDK();
            else
                Logger.LogError($"Adapter doesn't handle Banner", this);
        }

        protected virtual void ShowBannerSDK() { }

        public virtual void HideBanner() { }

        public virtual void DestroyBanner() { }
        #endregion

        #region AOA
        public bool IsAOAReady()
        {
            if (!IsInitialized)
            {
                Logger.LogError($"Adapter didn't initialize", this);
                return false;
            }

            if (_adsTypeUse.HasFlag(EAdsType.AOA))
            {
                return IsAOAReadySDK();
            }
            else
            {
                Logger.LogError($"Adapter doesn't handle AOA", this);
                return false;
            }
        }

        protected virtual bool IsAOAReadySDK() => false;

        public void LoadAOA()
        {
            if (!IsInitialized)
            {
                Logger.LogError($"Adapter didn't initialize", this);
                return;
            }

            if (_adsTypeUse.HasFlag(EAdsType.AOA))
                LoadAOASDK();
            else
                Logger.LogError($"Adapter doesn't handle AOA", this);
        }

        protected virtual void LoadAOASDK() { }

        public void ShowAOA() 
        {
            if (!IsInitialized)
            {
                Logger.LogError($"Adapter didn't initialize", this);
                return;
            }

            if (_adsTypeUse.HasFlag(EAdsType.AOA))
            {
                if (IsAOAReady())
                    ShowAOASDK();
            }
            else
            {
                Logger.LogError($"Adapter doesn't handle AOA", this);
            }
        }

        protected virtual void ShowAOASDK() { }
        #endregion
    }
}


