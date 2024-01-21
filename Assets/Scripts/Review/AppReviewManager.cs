using UnityEngine;
#if USE_IN_APP_REVIEW && UNITY_IOS
using UnityEngine.iOS;
#elif USE_IN_APP_REVIEW && UNITY_ANDROID
using Google.Play.Review;
#endif

namespace NFramework
{
    public class AppReviewManager : SingletonMono<AppReviewManager>
    {
        [SerializeField] private bool _initReviewAtStart;

#if USE_IN_APP_REVIEW && UNITY_ANDROID
        private ReviewManager _reviewManager;
        private PlayReviewInfo _playReviewInfo;
        private Coroutine _coroutine;
#endif

        private void Start()
        {
            if (_initReviewAtStart)
            {
#if USE_IN_APP_REVIEW && UNITY_ANDROID
            _coroutine = StartCoroutine(InitReview());
#endif
            }
        }

        public void RateAndReview(string appStoreId = "")
        {
#if USE_IN_APP_REVIEW && UNITY_IOS
        if (!Device.RequestStoreReview())
            DirectlyOpen(appStoreId);
#elif USE_IN_APP_REVIEW && UNITY_ANDROID
            StartCoroutine(LaunchReview());
#else
            DirectlyOpen(appStoreId);
#endif
        }

#if USE_IN_APP_REVIEW && UNITY_ANDROID
        private IEnumerator InitReview(bool force = false)
        {
            if (_reviewManager == null)
                _reviewManager = new ReviewManager();

            var requestFlowOperation = _reviewManager.RequestReviewFlow();
            yield return requestFlowOperation;
            if (requestFlowOperation.Error != ReviewErrorCode.NoError)
            {
                if (force)
                    DirectlyOpen();

                yield break;
            }

            _playReviewInfo = requestFlowOperation.GetResult();
        }

        private IEnumerator LaunchReview()
        {
            if (_playReviewInfo == null)
            {
                if (_coroutine != null)
                    StopCoroutine(_coroutine);

                yield return StartCoroutine(InitReview(true));
            }

            var launchFlowOperation = _reviewManager.LaunchReviewFlow(_playReviewInfo);
            yield return launchFlowOperation;
            _playReviewInfo = null;
            if (launchFlowOperation.Error != ReviewErrorCode.NoError)
            {
                DirectlyOpen();
                yield break;
            }
        }
#endif

        public void DirectlyOpen(string appStoreId = "")
        {
            if (DeviceInfo.IsAndroid)
                Application.OpenURL($"https://play.google.com/store/apps/details?id={Application.identifier}");
            else if (DeviceInfo.IsIOS)
                Application.OpenURL($"https://apps.apple.com/app/id{appStoreId}");
        }
    }

}
