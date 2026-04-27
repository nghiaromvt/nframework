using System.Threading;
using UnityEngine;

namespace NFramework
{
    public abstract class BaseCancellableBehaviour : MonoBehaviour
    {
        private CancellationTokenSource _disableCts;
        private CancellationTokenSource _disableOrDestroyCts;
        private CancellationTokenSource _linkedDisableCts;

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
        
        public CancellationToken DisableOrDestroyToken
        {
            get
            {
                // Chỉ tạo linked CTS mới khi _disableCts thay đổi (sau OnEnable/OnDisable)
                if (_disableOrDestroyCts == null || _linkedDisableCts != _disableCts)
                {
                    _disableOrDestroyCts?.Dispose();
                    _linkedDisableCts = _disableCts;
                    _disableOrDestroyCts = CancellationTokenSource.CreateLinkedTokenSource(
                        DisableCancellationToken, destroyCancellationToken);
                }
                return _disableOrDestroyCts.Token;
            }
        }

        protected virtual void OnEnable()
        {
            // Tạo mới CTS khi enable
            _disableCts?.Dispose();
            _disableCts = new CancellationTokenSource();
            
            // Invalidate linked CTS vì _disableCts đã thay đổi
            _disableOrDestroyCts?.Dispose();
            _disableOrDestroyCts = null;
            _linkedDisableCts = null;
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
            
            _disableOrDestroyCts?.Dispose();
            _disableOrDestroyCts = null;
            _linkedDisableCts = null;
        }
    }
}
