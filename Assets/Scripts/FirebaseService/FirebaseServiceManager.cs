#if USE_FIREBASE
using Firebase.Extensions;
#if USE_FIREBASE_CRASHLYTICS
using Firebase.Crashlytics;
#endif
#endif
using System;
using UnityEngine;

namespace NFramework.FirebaseService
{
    public static class FirebaseServiceManager
    {
        private static bool _isInitializing;
        private static Action _onInitSuccessCallback;

#if USE_FIREBASE
        public static Firebase.FirebaseApp App { get; private set; }
#endif
        public static bool IsInitialized { get; private set; }

        public static void CheckAndTryInit(Action callback)
        {
            if (IsInitialized)
            {
                callback?.Invoke();
            }
            else if (_isInitializing)
            {
                _onInitSuccessCallback += callback;
            }
            else
            {
                _isInitializing = true;
                _onInitSuccessCallback += callback;

                if (Application.isEditor)
                {
                    IsInitialized = true;
                    _onInitSuccessCallback?.Invoke();
                    _isInitializing = false;
                }
                else
                {
#if USE_FIREBASE
                    Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
                    {
                        var dependencyStatus = task.Result;
                        if (dependencyStatus == Firebase.DependencyStatus.Available)
                        {
                            IsInitialized = true;
                            App = Firebase.FirebaseApp.DefaultInstance;
#if USE_FIREBASE_CRASHLYTICS
                            Crashlytics.ReportUncaughtExceptionsAsFatal = true;
#endif
                            _onInitSuccessCallback?.Invoke();
                        }
                        else
                        {
                            Debug.LogError(string.Format(
                                "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
                            // Firebase Unity SDK is not safe to use here.
                        }
                        _isInitializing = false;
                    });
#endif
                        }
                    }
        }
    }
}

