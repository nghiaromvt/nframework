#if USE_FIREBASE
using Firebase.Extensions;
using System;
using UnityEngine;

namespace NFramework.Firebase
{
    public static class FirebaseServiceManager
    {
        private static bool _isInitializing;
        private static Action _onInitSuccessCallback;

        public static Firebase.FirebaseApp App { get; private set; }
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
                    Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
                    {
                        var dependencyStatus = task.Result;
                        if (dependencyStatus == Firebase.DependencyStatus.Available)
                        {
                            IsInitialized = true;
                            App = Firebase.FirebaseApp.DefaultInstance;
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
                }
            }
        }
    }
}
#endif

