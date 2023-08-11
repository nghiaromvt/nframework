using System;
using System.Collections;
using UnityEngine;

namespace NFramework
{
    public class InternetChecker : SingletonMono<InternetChecker>
    {
        public static event Action<bool> OnStatusChanged;

        private bool _internetStatus;

        public bool InternetStatus
        {
            get => _internetStatus;
            private set
            {
                if (_internetStatus != value)
                {
                    _internetStatus = value;
                    OnStatusChanged?.Invoke(value);
                }
            }
        }

        private void Start() => StartCoroutine(CRInternetCheck());

        private IEnumerator CRInternetCheck()
        {
            var wait = new WaitForSecondsRealtime(1f);
            while (true)
            {
                InternetStatus = DeviceInfo.HasInternet();
                yield return wait;
            }
        }
    }
}
