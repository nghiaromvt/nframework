using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace NFramework
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class TMPGreyscaleHandler : UIGreyscaleHandler
    {
        [SerializeField] private TextMeshProUGUI _tmp;
        [SerializeField] private bool _useColor;
        [SerializeField, HideIf(nameof(_useColor))] private Material _defaultMat;
        [SerializeField, HideIf(nameof(_useColor))] private Material _greyscaleMat;
        [SerializeField, ShowIf(nameof(_useColor))] private Color _defaultColor;
        [SerializeField, ShowIf(nameof(_useColor))] private Color _greyscaleColor;
        
        protected override void OnValidate()
        {
            if (_tmp == null)
                _tmp = GetComponent<TextMeshProUGUI>();

            if (!_useColor)
            {
                if (_defaultMat == null || !_defaultMat.name.Contains(_tmp.font.name))
                    _defaultMat = _tmp.fontSharedMaterial;
            
                if  (_greyscaleMat == null || !_greyscaleMat.name.Contains(_tmp.font.name))
                    _greyscaleMat = _tmp.fontSharedMaterial;
            }

            base.OnValidate();
        }

        public override void Toggle(bool isOn)
        {
            base.Toggle(isOn);

            if (_useColor)
            {
                _tmp.color = _isOn ? _greyscaleColor : _defaultColor;
            }
            else
            {
                _tmp.fontSharedMaterial = _isOn ? _greyscaleMat : _defaultMat;
            }
        }
    }
}