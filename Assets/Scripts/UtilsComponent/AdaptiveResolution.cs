using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System;
using UnityEngine.Events;

namespace NFramework
{
    public class AdaptiveResolution : MonoBehaviour
    {
        [System.Serializable]
        public class AdaptiveInfo
        {
            public string name;
            public float minAspect;
            public bool manualSetSize;
            [ConditionalField(nameof(manualSetSize))] public float manualOrthoSize = 10f;
            [ConditionalField(nameof(manualSetSize))] public float manualPerspectiveSize = 60f;
        }

        public static event Action<float, float> OnAdapt; // <CurOrthoSize,CurPerspectiveSize>

        [Header("Base")]
        [SerializeField] private float _baseAspect = 16f / 9;
        [SerializeField] private float _baseOrthoSize = 10f;
        [SerializeField] private float _basePerspectiveSize = 60f;
        [SerializeField] private bool _isInvert;

        [Header("Infos")]
        [SerializeField] private List<AdaptiveInfo> _adaptiveInfos = new List<AdaptiveInfo>()
        {
            new AdaptiveInfo { name = "IphoneX", minAspect = 2f }, // ipX - 19.5:9 = 2.167f
            new AdaptiveInfo { name = "Iphone8", minAspect = 1.7f }, // ip8 - 16:9 = 1.778f
            new AdaptiveInfo { name = "Ipad", minAspect = 1.3f }, // ipad - 4:3 = 1.333f
        };

        [Header("Event")]
        public UnityEvent<float> OnAdaptOrthoSize;
        public UnityEvent<float> OnAdaptPerspectiveSize;

        public float CurAspect { get; private set; }
        public float AspectScale { get; private set; }
        public float CurOrthoSize { get; private set; }
        public float CurPerspectiveSize { get; private set; }

        private void Awake()
        {
            _adaptiveInfos = _adaptiveInfos.OrderByDescending(info => info.minAspect).ToList();
            Adapt();
        }

#if UNITY_EDITOR
        private void FixedUpdate() => Adapt();
#endif

        private void Adapt()
        {
            var aspect = 0f;
            if (Screen.width > Screen.height) // Landscape
                aspect = (float)Screen.width / Screen.height;
            else // Portrait
                aspect = (float)Screen.height / Screen.width;

            if (aspect == CurAspect)
                return;

            CurAspect = aspect;
            AspectScale = CurAspect / _baseAspect;
            if (_isInvert)
                AspectScale = 1 / AspectScale;

            var adaptiveInfo = GetAdaptiveInfo(CurAspect);
            if (adaptiveInfo == null)
                return;

            if (adaptiveInfo.manualSetSize)
            {
                CurOrthoSize = adaptiveInfo.manualOrthoSize;
                CurPerspectiveSize = adaptiveInfo.manualPerspectiveSize;
            }
            else
            {
                CurOrthoSize = _baseOrthoSize * AspectScale;
                CurPerspectiveSize = _basePerspectiveSize * AspectScale;
            }

            OnAdapt?.Invoke(CurOrthoSize, CurPerspectiveSize);
            OnAdaptOrthoSize?.Invoke(CurOrthoSize);
            OnAdaptPerspectiveSize?.Invoke(CurPerspectiveSize);
        }

        private AdaptiveInfo GetAdaptiveInfo(float curAspect)
        {
            foreach (var info in _adaptiveInfos)
            {
                if (curAspect > info.minAspect)
                    return info;
            }
            Logger.LogError($"Cannot GetAdaptiveInfo({curAspect})", this);
            return null;
        }     
    }
}

