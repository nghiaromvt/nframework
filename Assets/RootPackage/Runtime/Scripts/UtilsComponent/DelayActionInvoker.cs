using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace NFramework
{
    public class DelayActionInvoker : BaseCancellableBehaviour
    {
        private enum InvokeAt { OnEnable, Start, Custom }
        private enum CancelMode { OnDisable, OnDestroy }

        public UnityEvent OnInvoke;

        [SerializeField] private float _delay;
        [SerializeField] private bool _useRealtime;
        [SerializeField] private InvokeAt _invokeAt = InvokeAt.OnEnable;
        [SerializeField] private CancelMode _cancelMode = CancelMode.OnDisable;

        protected override void OnEnable()
        {
            base.OnEnable();

            if (_invokeAt == InvokeAt.OnEnable)
                InvokeDelayAsync().Forget();
        }

        private void Start()
        {
            if (_invokeAt == InvokeAt.Start)
                InvokeDelayAsync().Forget();
        }

        // For call from inspector or code when InvokeAt is Custom
        public void Invoke() => InvokeDelayAsync().Forget();

        // For call from inspector
        public void DestroyGameObject() => Destroy(gameObject);

        private async UniTaskVoid InvokeDelayAsync()
        {
            var ct = _cancelMode == CancelMode.OnDisable
                ? DisableOrDestroyToken
                : destroyCancellationToken;

            if (_useRealtime)
                await UniTask.Delay(TimeSpan.FromSeconds(_delay), DelayType.Realtime, cancellationToken: ct);
            else
                await UniTask.Delay(TimeSpan.FromSeconds(_delay), DelayType.DeltaTime, cancellationToken: ct);

            OnInvoke?.Invoke();
        }
    }
}
