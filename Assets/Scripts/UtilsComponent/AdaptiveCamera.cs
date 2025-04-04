using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace NFramework
{
    [RequireComponent(typeof(Camera))]
    public class AdaptiveCamera : MonoBehaviour
    {
        [Serializable]
        public class ManualInfo
        {
            [MinMaxSlider(0, 3, true)] public Vector2 aspectRatioRange;
            public float orthoSize;
            public float pov;
        }
        
        [HideInInspector, SerializeField] private Camera _camera;
        [FoldoutGroup("General"), SerializeField] private bool _adaptAtAwake;
        [FoldoutGroup("General"), SerializeField] private bool _adaptContinuity;
        [FoldoutGroup("General"), SerializeField] private bool _isOrtho;
        [FoldoutGroup("General"), SerializeField] private float _baseAspectRatio = 16f / 9;
        [FoldoutGroup("General"), ShowIf(nameof(_isOrtho)), SerializeField] private float _baseOrthoSize = 10f;
        [FoldoutGroup("General"), HideIf(nameof(_isOrtho)), SerializeField] private float _baseFov = 60f;
        [SerializeField] private List<ManualInfo> _manualInfos = new();
        
        private float _adaptedAspectRatio;
        
        private void OnValidate() => _camera ??= GetComponent<Camera>();

        private void Awake()
        {
            if (_adaptAtAwake) Adapt();
        }

        private IEnumerator Start()
        {
            while (_adaptContinuity)
            {
                Adapt();
                yield return null;
            }
        }

        [Button]
        public void Adapt()
        {
            var curAspectRatio = Screen.width > Screen.height 
                ? (float)Screen.width / Screen.height // Landscape
                : (float)Screen.height / Screen.width; // Portrait

            if (Mathf.Approximately(_adaptedAspectRatio, curAspectRatio))
                return;
            
            _adaptedAspectRatio = curAspectRatio;
            
            foreach (var manualInfo in _manualInfos)
            {
                if (manualInfo.aspectRatioRange.x <= curAspectRatio && manualInfo.aspectRatioRange.y >= curAspectRatio)
                {
                    if (_isOrtho)
                        _camera.orthographicSize = manualInfo.orthoSize;
                    else
                        _camera.fieldOfView = manualInfo.pov;
                    
                    return;
                }
            }
            
            var aspectScale = curAspectRatio / _baseAspectRatio;
            
            if (_isOrtho)
                _camera.orthographicSize = _baseOrthoSize * aspectScale;
            else
                _camera.fieldOfView = _baseFov * aspectScale;
        }
    }
}
