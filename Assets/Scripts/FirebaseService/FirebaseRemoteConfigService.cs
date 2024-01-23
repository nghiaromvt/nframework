#if USE_FIRESEBASE && USE_FIREBASE_REMOTECONFIG 
using Firebase.Extensions;
using Firebase.RemoteConfig;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace NFramework.FirebaseService
{
    public static class FirebaseRemoteConfigService
    {
        public static event Action OnFetchSuccess;

        public static bool IsInitialized { get; private set; }
        public static bool IsFetchSuccess { get; private set; }
        public static bool IsFetching { get; private set; }

        public static void Init(Dictionary<string, object> defaults)
        {
            FirebaseServiceManager.CheckAndTryInit(() =>
            {
                FirebaseRemoteConfig.DefaultInstance.SetDefaultsAsync(defaults).ContinueWithOnMainThread(task =>
                {
                    IsInitialized = true;
                    FetchDataAsync();
                });
            });
        }

        public static Task FetchDataAsync()
        {
            if (IsFetching)
                return null;

            Debug.Log("[FirebaseRemoteConfigService] Fetching data...");
            IsFetching = true;
            var fetchTask = FirebaseRemoteConfig.DefaultInstance.FetchAsync(TimeSpan.Zero);
            return fetchTask.ContinueWithOnMainThread(FetchComplete);
        }

        private static void FetchComplete(Task fetchTask)
        {
            if (!fetchTask.IsCompleted)
            {
                Debug.LogError("[FirebaseRemoteConfigService] Retrieval hasn't finished.");
                return;
            }

            IsFetching = false;
            var remoteConfig = FirebaseRemoteConfig.DefaultInstance;
            var info = remoteConfig.Info;
            if (info.LastFetchStatus != LastFetchStatus.Success)
            {
                Debug.LogError($"[FirebaseRemoteConfigService] Fetch was unsuccessful\n{nameof(info.LastFetchStatus)}: {info.LastFetchStatus}");
                IsFetchSuccess = false;
                return;
            }

            // Fetch successful. Parameter values must be activated to use.
            remoteConfig.ActivateAsync().ContinueWithOnMainThread(task =>
            {
                Debug.Log($"[FirebaseRemoteConfigService] Remote data loaded and ready for use. Last fetch time {info.FetchTime}.");
                IsFetchSuccess = true;
                OnFetchSuccess?.Invoke();
            });
        }
    }

    // GameRemoteConfig should override this class
    public class RemoteConfig : MonoBehaviour
    {
        [SerializeField] protected bool _autoInitAtStart = true;

        protected virtual void Start()
        {
            if (_autoInitAtStart)
            {
                FirebaseRemoteConfigService.Init(GetDefaultsDic());
                FirebaseRemoteConfigService.OnFetchSuccess += FirebaseRemoteConfigService_OnFetchSuccess;
                InternetChecker.OnStatusChanged += InternetChecker_OnStatusChanged;
            }   
        }

        protected virtual void OnDestroy()
        {
            FirebaseRemoteConfigService.OnFetchSuccess -= FirebaseRemoteConfigService_OnFetchSuccess;
            InternetChecker.OnStatusChanged -= InternetChecker_OnStatusChanged;
        }

        private void InternetChecker_OnStatusChanged(bool status)
        {
            if (status && !FirebaseRemoteConfigService.IsFetchSuccess)
                FirebaseRemoteConfigService.FetchDataAsync();
        }

        // Get default local values. ex: { "inter_start_level", InterStartLevel }
        protected virtual Dictionary<string, object> GetDefaultsDic() => new Dictionary<string, object>();

        // Set local values after fetch success. ex: InterStartLevel = (int)FirebaseRemoteConfig.DefaultInstance.GetValue("inter_start_level").DoubleValue;
        protected virtual void FirebaseRemoteConfigService_OnFetchSuccess() { }

    }
}
#endif
