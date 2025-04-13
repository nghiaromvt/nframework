using UnityEngine;
using System;
#if MOREMOUNTAINS_NICEVIBRATIONS_INSTALLED
using Lofelt.NiceVibrations;
#endif

namespace NFramework
{
    public class VibrationManager : SingletonMono<VibrationManager>, ISaveable
    {
        public enum EHapticType 
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

        public static event Action<bool> OnStatusChanged;

        [SerializeField] private SaveData _saveData;

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

        public static void Haptic(EHapticType type)
        {
            if (!I.Status) return;

#if MOREMOUNTAINS_NICEVIBRATIONS_INSTALLED
            HapticPatterns.PlayPreset((HapticPatterns.PresetType)type);
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
            _saveData = string.IsNullOrEmpty(data) ? new SaveData() : JsonUtility.FromJson<SaveData>(data);
            OnStatusChanged?.Invoke(Status);
        }

        public void OnAllDataLoaded() { }
        
        #endregion
    }
}