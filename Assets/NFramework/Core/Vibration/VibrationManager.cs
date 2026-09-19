using UnityEngine;
using System;
using System.Collections.Generic;
#if MOREMOUNTAINS_NICEVIBRATIONS
using MoreMountains.NiceVibrations;
#endif

namespace NFramework
{
    public class VibrationManager : SingletonMono<VibrationManager>
    {
        public enum HapticType 
        { 
            Selection = 0, 
            Success = 1, 
            Warning = 2, 
            Failure = 3, 
            LightImpact = 4, 
            MediumImpact = 5, 
            HeavyImpact = 6, 
            RigidImpact = 7, 
            SoftImpact = 8, 
            None = -1,
        }

        [Serializable]
        public class HapticSettings
        {
            public float restTime;
        }

        public static event Action<bool> OnStatusChanged;

        [SerializeField] private UnitySerializedDictionary<HapticType, HapticSettings> _hapticSettingsDict = new();

        private Dictionary<HapticType, float> _lastHapticTimeDict = new();
        private bool _status;
        
        public bool Status
        {
            get => _status;
            set
            {
                if (_status != value)
                {
                    _status = value;
                    OnStatusChanged?.Invoke(value);
                }
            }
        }

        public void Haptic(HapticType type)
        {
            if (!Status)
                return;

            if (_hapticSettingsDict.TryGetValue(type, out var settings))
            {
                var lastHapticTime = _lastHapticTimeDict.GetOrAdd(type, Time.time);
                if (Time.time < lastHapticTime + settings.restTime)
                    return;
                
                _lastHapticTimeDict[type] = Time.time;
            }

#if MOREMOUNTAINS_NICEVIBRATIONS
            MMVibrationManager.Haptic((HapticTypes)type);
#endif
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            OnStatusChanged = null;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatic()
        {
            OnStatusChanged = null;
        }
    }
}