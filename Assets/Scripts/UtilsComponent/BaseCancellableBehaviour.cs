using System.Threading;
using UnityEngine;

namespace NFramework
{
    public abstract class BaseCancellableBehaviour : MonoBehaviour
    {
        private CancellationTokenSource _disableCts;

        public CancellationToken DisableCancellationToken
        {
            get
            {
                if (_disableCts == null || _disableCts.IsCancellationRequested)
                {
                    _disableCts?.Dispose();
                    _disableCts = new CancellationTokenSource();
                }
                return _disableCts.Token;
            }
        }
        
        public CancellationToken DisableOrDestroyToken => 
            CancellationTokenSource.CreateLinkedTokenSource(DisableCancellationToken, destroyCancellationToken).Token;

        protected virtual void OnEnable()
        {
            // Tạo mới CTS khi enable
            _disableCts?.Dispose();
            _disableCts = new CancellationTokenSource();
        }

        protected virtual void OnDisable()
        {
            _disableCts?.Cancel();
            // Không Dispose ngay, để token vẫn valid trong lúc await đang chạy
        }

        protected virtual void OnDestroy()
        {
            _disableCts?.Cancel();
            _disableCts?.Dispose();
            _disableCts = null;
        }
    }
}
