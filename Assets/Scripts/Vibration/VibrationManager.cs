using UnityEngine;
using System;
#if MOREMOUNTAINS_NICEVIBRATIONS_INSTALLED
using Lofelt.NiceVibrations;
#endif

namespace NFramework
{
    public class VibrationManager : SingletonMono<VibrationManager>, ISaveable
    {
        public enum EHapticType { Selection, Success, Warning, Failure, LightImpact, MediumImpact, HeavyImpact, RigidImpact, SoftImpact, None }

        public static Action<bool> OnStatusChanged;

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

        public void Haptic(EHapticType type)
        {
            if (!Status)
                return;

#if MOREMOUNTAINS_NICEVIBRATIONS_INSTALLED
            HapticPatterns.PlayPreset((HapticPatterns.PresetType)type);
#endif
        }

        #region ISaveable
        [System.Serializable]
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
