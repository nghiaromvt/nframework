using System.Threading;
using Cysharp.Threading.Tasks;
using PrimeTween;
using UnityEngine;

namespace NFramework.UI
{
    [RequireComponent(typeof(CanvasGroup), typeof(UIView))]
    public class FadeViewAnimation : MonoBehaviour, IOpenViewAnimation, ICloseViewAnimation
    {
        #region Fields

        [SerializeField] private float _fadeInDuration = 0.25f;
        [SerializeField] private float _fadeOutDuration = 0.15f;
        [SerializeField] private Ease _fadeInEase = Ease.OutQuad;
        [SerializeField] private Ease _fadeOutEase = Ease.InQuad;
        [SerializeField] private bool _blockInteract = true;

        private CanvasGroup _canvasGroup;
        private Tween _tween;

        #endregion

        #region Properties

        public bool BlockInteract => _blockInteract;

        #endregion

        #region Unity Lifecycle Functions

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        private void OnDisable()
        {
            _tween.Stop();
        }

        #endregion

        #region Custom Methods

        public async UniTask PlayOpen(CancellationToken cancellationToken = default)
        {
            _tween.Stop();
            _canvasGroup.alpha = 0f;

            _tween = Tween.Alpha(_canvasGroup, 0f, 1f, _fadeInDuration, _fadeInEase);
            await _tween.ToYieldInstruction().ToUniTask(cancellationToken: cancellationToken);
        }

        public async UniTask PlayClose(CancellationToken cancellationToken = default)
        {
            _tween.Stop();

            _tween = Tween.Alpha(_canvasGroup, 1f, 0f, _fadeOutDuration, _fadeOutEase);
            await _tween.ToYieldInstruction().ToUniTask(cancellationToken: cancellationToken);
        }

        #endregion
    }
}
