using System.Threading;
using UnityEngine;

namespace NFramework
{
    [AddComponentMenu("")]
    internal sealed class ActiveCancellationTokenTracker : MonoBehaviour
    {
        private CancellationTokenSource _cts;

        public CancellationToken Token
        {
            get
            {
                if (_cts == null || _cts.IsCancellationRequested)
                {
                    _cts?.Dispose();
                    _cts = new CancellationTokenSource();
                }
                return _cts.Token;
            }
        }

        private void OnEnable()
        {
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
        }

        private void OnDisable()
        {
            _cts?.Cancel();
        }

        private void OnDestroy()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
        }
    }
}
