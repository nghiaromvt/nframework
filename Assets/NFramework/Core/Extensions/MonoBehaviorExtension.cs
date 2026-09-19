using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace NFramework
{
    public static class MonoBehaviorExtension
    {
        public static Coroutine InvokeDelay(this MonoBehaviour self, float delay, Action action)
        {
            return self.StartCoroutine(CoroutineHelper.CRDelayAction(delay, action));
        }

        public static Coroutine InvokeDelayRealtime(this MonoBehaviour self, float delay, Action action)
        {
            return self.StartCoroutine(CoroutineHelper.CRDelayRealtimeAction(delay, action));
        }

        public static Coroutine InvokeDelayFrame(this MonoBehaviour self, int frameCount, Action action)
        {
            return self.StartCoroutine(CoroutineHelper.CRDelayFrameAction(frameCount, action));
        }

        public static Coroutine InvokeDelayUntil(this MonoBehaviour self, Func<bool> predicate, Action action)
        {
            return self.StartCoroutine(CoroutineHelper.CRDelayUntilAction(predicate, action));
        }

        public static async UniTask DelayAsync(this MonoBehaviour self, float delay, Action action, CancellationToken cancellationToken = default)
        {
            var token = cancellationToken == CancellationToken.None ? self.GetCancellationTokenOnDestroy() : cancellationToken;
            await UniTask.Delay(TimeSpan.FromSeconds(delay), DelayType.DeltaTime, cancellationToken: token);
            action?.Invoke();
        }

        public static async UniTask DelayRealtimeAsync(this MonoBehaviour self, float delay, Action action, CancellationToken cancellationToken = default)
        {
            var token = cancellationToken == CancellationToken.None ? self.GetCancellationTokenOnDestroy() : cancellationToken;
            await UniTask.Delay(TimeSpan.FromSeconds(delay), DelayType.Realtime, cancellationToken: token);
            action?.Invoke();
        }

        public static async UniTask DelayFrameAsync(this MonoBehaviour self, int frameCount, Action action, CancellationToken cancellationToken = default)
        {
            var token = cancellationToken == CancellationToken.None ? self.GetCancellationTokenOnDestroy() : cancellationToken;
            await UniTask.DelayFrame(frameCount, cancellationToken: token);
            action?.Invoke();
        }

        public static async UniTask DelayUntilAsync(this MonoBehaviour self, Func<bool> predicate, Action action, CancellationToken cancellationToken = default)
        {
            var token = cancellationToken == CancellationToken.None ? self.GetCancellationTokenOnDestroy() : cancellationToken;
            await UniTask.WaitUntil(predicate, cancellationToken: token);
            action?.Invoke();
        }

        /// <summary>
        /// Returns a <see cref="System.Threading.CancellationToken"/> that is cancelled when the
        /// MonoBehaviour's GameObject is deactivated (<c>SetActive(false)</c>) or destroyed.
        /// The token is automatically renewed each time the GameObject is re-activated.
        /// </summary>
        public static CancellationToken GetCancellationTokenOnDisable(this MonoBehaviour self)
        {
            return self.gameObject.GetOrAddComponent<ActiveCancellationTokenTracker>().Token;
        }

        public static void Log(this MonoBehaviour self, object message, Color? color = null)
        {
            NLogger.Log(message, self, color);
        }

        public static void LogWarning(this MonoBehaviour self, object message, Color? color = null)
        {
            NLogger.LogWarning(message, self, color);
        }

        public static void LogError(this MonoBehaviour self, object message, Color? color = null)
        {
            NLogger.LogError(message, self, color);
        }

        public static void LogAssert(this MonoBehaviour self, bool condition, object message, Color? color = null)
        {
            NLogger.LogAssert(condition, message, self, color);
        }
    }
}