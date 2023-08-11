using UnityEngine;
using System;
#if MOREMOUNTAINS_NICEVIBRATIONS
using MoreMountains.NiceVibrations;
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

        public void Haptic(EHapticType type, bool defaultToRegularVibrate = false, bool alsoRumble = false, MonoBehaviour coroutineSupport = null, int controllerID = -1)
        {
            if (!Status)
                return;

#if MOREMOUNTAINS_NICEVIBRATIONS
            MMVibrationManager.Haptic((HapticTypes)type, defaultToRegularVibrate, alsoRumble, coroutineSupport, controllerID);
#endif
        }

        public void TransientHaptic(float intensity, float sharpness, bool alsoRumble = false, MonoBehaviour coroutineSupport = null, int controllerID = -1)
        {
            if (!Status)
                return;

#if MOREMOUNTAINS_NICEVIBRATIONS
            MMVibrationManager.TransientHaptic(intensity, sharpness, alsoRumble, coroutineSupport, controllerID);
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
