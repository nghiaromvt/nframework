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
        private Coroutine _crDelayResetIsFullscreenAdShowing;

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
                    IsFullscreenAdShowing = true;
                    _config.adsCallbackListener?.OnRequestShowInter();
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

        protected void HandleInterClicked() => _config.adsCallbackListener?.OnInterClicked();

        protected void HandleInterLoaded()
        {
            _interLoadRetryAttempt = 0;
            _config.adsCallbackListener?.OnInterLoaded();
        }

        protected void HandleInterLoadFailed(string error = "")
        {
            Debug.LogError($"HandleInterLoadFailed error:{error}", this);
            _interLoadRetryAttempt++;
            var retryDelay = Mathf.Pow(2, Mathf.Min(6, _interLoadRetryAttempt));
            this.InvokeDelayRealtime(retryDelay, LoadInter);
            _config.adsCallbackListener?.OnInterLoadFailed();
        }

        protected void HandleInterDisplayed() => _config.adsCallbackListener?.OnInterDisplayed();

        protected void HandleInterDisplayFailed(string error = "")
        {
            Debug.LogError($"HandleInterDisplayFailed error:{error}", this);
            DelayResetIsFullscreenAdShowing();
            if (_cachedAdsShowDataDic.TryGetValue(EAdsType.Inter, out var data) && data != null)
            {
                data.callback?.Invoke(false);
                _cachedAdsShowDataDic.Remove(EAdsType.Inter);
            }
            LoadInter();
        }

        protected void HandleInterHidden()
        {
            DelayResetIsFullscreenAdShowing();
            if (_cachedAdsShowDataDic.TryGetValue(EAdsType.Inter, out var data) && data != null)
            {
                data.callback?.Invoke(true);
                _cachedAdsShowDataDic.Remove(EAdsType.Inter);
            }
            LoadInter();
        }
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
                    IsFullscreenAdShowing = true;
                    _config.adsCallbackListener?.OnRequestShowReward();
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

        protected void HandleRewardClicked() => _config.adsCallbackListener?.OnRewardClicked();

        protected void HandleRewardLoadFailed(string error = "")
        {
            Debug.LogError($"HandleRewardLoadFailed error:{error}", this);
            _rewardLoadRetryAttempt++;
            var retryDelay = Mathf.Pow(2, Mathf.Min(6, _rewardLoadRetryAttempt));
            this.InvokeDelayRealtime(retryDelay, LoadReward);
            _config.adsCallbackListener?.OnRewardLoadFailed();
        }

        protected void HandleRewardDisplayed() => _config.adsCallbackListener?.OnRewardDisplayed();

        protected void HandleRewardDisplayFailed(string error = "")
        {
            Debug.LogError($"HandleRewardDisplayFailed error:{error}", this);
            DelayResetIsFullscreenAdShowing();
            if (_cachedAdsShowDataDic.TryGetValue(EAdsType.Reward, out var data) && data != null)
            {
                data.callback?.Invoke(false);
                _cachedAdsShowDataDic.Remove(EAdsType.Reward);
            }
            _config.adsCallbackListener?.OnRewardDisplayFailed();
        }

        protected void HandleRewardRecieved()
        {
            if (_cachedAdsShowDataDic.TryGetValue(EAdsType.Reward, out var data) && data != null)
                data.haveReward = true;

            _config.adsCallbackListener?.OnRewardRecieved();
        }

        protected void HandleRewardHidden()
        {
            DelayResetIsFullscreenAdShowing();
            if (_cachedAdsShowDataDic.TryGetValue(EAdsType.Reward, out var data) && data != null)
            {
                data.callback?.Invoke(data.haveReward);
                _cachedAdsShowDataDic.Remove(EAdsType.Reward);
            }
        }

        protected void HandleRewardAvailable() => _config.adsCallbackListener?.OnRewardLoaded();
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

        protected void HandleBannerLoadFailed(string error = "") => Debug.LogError($"HandleBannerLoadFailed error:{error}", this);
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
                {
                    IsFullscreenAdShowing = true;
                    ShowAOASDK();
                }
            }
            else
            {
                Logger.LogError($"Adapter doesn't handle AOA", this);
            }
        }

        protected virtual void ShowAOASDK() { }

        protected void HandleAOALoadFailed(string error = "")
        {
            Debug.LogError($"HandleAOALoadFailed error:{error}", this);
            _aoaLoadRetryAttempt++;
            var retryDelay = Mathf.Pow(2, Mathf.Min(6, _aoaLoadRetryAttempt));
            this.InvokeDelayRealtime(retryDelay, LoadAOA);
        }

        protected void HandleAOALoaded() => _aoaLoadRetryAttempt = 0;

        protected void HandleAOADisplayFailed(string error = "")
        {
            Debug.LogError($"HandleAOADisplayFailed error:{error}");
            DelayResetIsFullscreenAdShowing();
            LoadAOA();
        }

        protected void HandleAOAHidden()
        {
            DelayResetIsFullscreenAdShowing();
            LoadAOA();
        }

        protected void HandleAOADisplayed() => _config.adsCallbackListener?.OnAOADisplayed();
        #endregion

        protected void HandleAdsRevenuePaid(AdsRevenueData data) => _config.adsCallbackListener?.OnAdsRevenuePaid(data);

        protected void DelayResetIsFullscreenAdShowing()
        {
            if (_crDelayResetIsFullscreenAdShowing != null)
                StopCoroutine(_crDelayResetIsFullscreenAdShowing);

            _crDelayResetIsFullscreenAdShowing = this.InvokeDelayRealtime(1f, () =>
            {
                IsFullscreenAdShowing = false;
                _crDelayResetIsFullscreenAdShowing = null;
            });
        }
    }
}


