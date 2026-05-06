using UnityEngine;
using System;
using System.Collections.Generic;
#if MOREMOUNTAINS_NICEVIBRATIONS
using MoreMountains.NiceVibrations;
#endif

namespace NFramework
{
    public class VibrationManager : SingletonMono<VibrationManager>, ISaveable
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

        [SerializeField] private SaveData _saveData;
        [SerializeField] private UnitySerializedDictionary<HapticType, HapticSettings> _hapticSettingsDict = new();

        private Dictionary<HapticType, float> _lastHapticTimeDict = new();
        
        public bool Status
        {
            get => _saveData.status;
            set
            {
                if (_saveData.status != value)
                {
                    _saveData.status = value;
                    DataChanged = true;
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

        #region ISaveable
        
        [Serializable]
        public class SaveData
        {
            public bool status = true;
        }

        public string SaveKey => "VibrationManager";

        public bool DataChanged { get; set; }

        public object GetData() => _saveData;

        public void SetData(string data)
        {
            if (string.IsNullOrEmpty(data))
                _saveData = new SaveData();
            else
                _saveData = JsonUtility.FromJson<SaveData>(data);

            OnStatusChanged?.Invoke(Status);
        }

        public void OnAllDataLoaded() { }
        
        #endregion
    }
}