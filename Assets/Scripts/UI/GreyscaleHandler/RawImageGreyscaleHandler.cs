using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace NFramework
{
    [RequireComponent(typeof(RawImage))]
    public class RawImageGreyscaleHandler : UIGreyscaleHandler
    {
        [SerializeField] private RawImage _image;
        [SerializeField] private bool _useColor;
        [SerializeField, HideIf(nameof(_useColor))] private Material _defaultMat;
        [SerializeField, HideIf(nameof(_useColor))] private Material _greyscaleMat;
        [SerializeField, ShowIf(nameof(_useColor))] private Color _defaultColor;
        [SerializeField, ShowIf(nameof(_useColor))] private Color _greyscaleColor;
        
        protected override void OnValidate()
        {
            if (_image == null)
                _image = GetComponent<RawImage>();

            if (!_useColor)
            {
                if (_defaultMat == null)
                    _defaultMat = _image.material;
            
                if (_greyscaleMat == null)
                    _greyscaleMat = Resources.Load<Material>(GREYSCALE_MAT_RESOURCES_PATH); 
            }

            base.OnValidate();
        }

        public override void Toggle(bool isOn)
        {
            base.Toggle(isOn);
            
            if (_useColor)
            {
                _image.color = _isOn ? _greyscaleColor : _defaultColor;
            }
            else
            {
                _image.material = _isOn ? _greyscaleMat : _defaultMat;
            }
        }
    }
}