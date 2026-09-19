using System.Threading;
using Cysharp.Threading.Tasks;
using PrimeTween;
using UnityEngine;

namespace NFramework.UI
{
    [RequireComponent(typeof(UIView))]
    public class ScaleViewAnimation : MonoBehaviour, IOpenViewAnimation, ICloseViewAnimation
    {
        #region Fields

        [SerializeField] private Transform _targetTransform;
        [SerializeField] private float _openDuration = 0.25f;
        [SerializeField] private float _closeDuration = 0.15f;
        [SerializeField] private Ease _openEase = Ease.OutBack;
        [SerializeField] private Ease _closeEase = Ease.InBack;
        [SerializeField] private float _openStartScaleMultiplier = 0f;
        [SerializeField] private float _closeEndScaleMultiplier = 0f;
        [SerializeField] private bool _blockInteract = true;

        private Vector3 _initialScale;
        private Tween _tween;

        #endregion

        #region Properties

        public bool BlockInteract => _blockInteract;

        #endregion

        #region Unity Lifecycle Functions

        private void Awake()
        {
            if (_targetTransform == null)
            {
                _targetTransform = transform;
            }

            _initialScale = _targetTransform.localScale;
        }

        private void OnDisable()
        {
            _tween.Stop();
            _targetTransform.localScale = _initialScale;
        }

        private void OnDestroy()
        {
            _targetTransform.localScale = _initialScale;
        }

        #endregion

        #region Custom Methods

        public async UniTask PlayOpen(CancellationToken cancellationToken = default)
        {
            _tween.Stop();
            _targetTransform.localScale = _initialScale * _openStartScaleMultiplier;

            _tween = Tween.Scale(_targetTransform, _initialScale, _openDuration, _openEase);
            await _tween.ToYieldInstruction().ToUniTask(cancellationToken: cancellationToken);
        }

        public async UniTask PlayClose(CancellationToken cancellationToken = default)
        {
            _tween.Stop();

            Vector3 endScale = _initialScale * _closeEndScaleMultiplier;
            _tween = Tween.Scale(_targetTransform, endScale, _closeDuration, _closeEase);
            await _tween.ToYieldInstruction().ToUniTask(cancellationToken: cancellationToken);
        }

        #endregion
    }
}
