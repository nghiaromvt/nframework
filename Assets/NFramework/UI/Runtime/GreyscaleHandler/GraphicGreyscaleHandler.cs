using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace NFramework.UI
{
    public abstract class GraphicGreyscaleHandler<T> : UIGreyscaleHandler where T : Graphic
    {
        [SerializeField] protected T _graphic;
        [SerializeField] protected bool _useColor;
        [SerializeField, HideIf(nameof(_useColor))] protected Material _defaultMat;
        [SerializeField, HideIf(nameof(_useColor))] protected Material _greyscaleMat;
        [SerializeField, ShowIf(nameof(_useColor))] protected Color _defaultColor;
        [SerializeField, ShowIf(nameof(_useColor))] protected Color _greyscaleColor;

        protected override void OnValidate()
        {
            if (_graphic == null)
                _graphic = GetComponent<T>();

            if (!_useColor)
            {
                if (_defaultMat == null && _graphic != null)
                    _defaultMat = _graphic.material;

                if (_greyscaleMat == null)
                {
                    NLogger.LogError($"[{GetType().Name}] Greyscale material is not assigned.", this);
                    return;
                }
            }

            base.OnValidate();
        }

        public override void Toggle(bool isOn)
        {
            base.Toggle(isOn);

            if (_graphic == null)
                return;

            if (_useColor)
            {
                _graphic.color = _isOn ? _greyscaleColor : _defaultColor;
            }
            else
            {
                _graphic.material = _isOn ? _greyscaleMat : _defaultMat;
            }
        }
    }
}
