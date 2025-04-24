using Newtonsoft.Json;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using UnityEngine;

namespace NFramework
{
    // All ISaveable must register to SaveManager before it Load
    // Set DataChanged to true if data is changed
    public interface ISaveable
    {
        string SaveKey { get; }
        bool DataChanged { get; set; }
        object GetData();
        void SetData(string data);
        void OnAllDataLoaded();
    }

    public class LocalSaveManager : SingletonMono<LocalSaveManager>
    {
        private const string SAVE_NAME = "mwovjtpamcjaytifnhyqlbprths";
        private const string BACKUP_SAVE_NAME = "_" + SAVE_NAME;

        public static event Action<string> OnSave;

        private static Dictionary<string, ISaveable> _saveableDict = new();

        [SerializeField] private bool _autoSave = true;
        [ShowIf(nameof(_autoSave))]
        [SerializeField] private float _autoSaveInterval = 5f;

        private bool _needSaveInterrupt = true;

        private IEnumerator Start()
        {
            var wait = new WaitForSecondsRealtime(_autoSaveInterval);
            while (_autoSave)
            {
                yield return wait;
                Save();
            }
        }

        private void OnApplicationFocus(bool focus)
        {
            if (focus)
            {
                if (!_needSaveInterrupt)
                    _needSaveInterrupt = true;
            }
            else
            {
                if (_needSaveInterrupt)
                {
                    _needSaveInterrupt = false;
                    Save();
                }
            }
        }

        private void OnApplicationPause(bool pause)
        {
            if (pause)
            {
                if (_needSaveInterrupt)
                {
                    _needSaveInterrupt = false;
                    Save();
                }
            }
            else
            {
                if (!_needSaveInterrupt)
                    _needSaveInterrupt = true;
            }
        }

        private void OnApplicationQuit() => Save();

        public static void RegisterSaveData(ISaveable data) => _saveableDict[data.SaveKey] = data;

        public static bool Save(bool hasBackup = true)
        {
            bool result = false;
            try
            {
                bool hasChanged = false;
                foreach (string key in _saveableDict.Keys)
                {
                    hasChanged = _saveableDict[key].DataChanged;
                    if (hasChanged)
                        break;
                }

                if (hasChanged)
                {
                    Dictionary<string, string> temp = new();
                    bool checkValid = false;
                    foreach (string key in _saveableDict.Keys)
                    {
                        temp[key] = JsonConvert.SerializeObject(_saveableDict[key].GetData());
                        checkValid = true;
                        _saveableDict[key].DataChanged = false;
                    }

                    if (checkValid)
                    {
                        var dataJson = JsonConvert.SerializeObject(temp);
                        byte[] dataBytes = System.Text.Encoding.UTF8.GetBytes(dataJson);

                        if (DeviceHelper.IsWebGL)
                            SaveToPlayerPrefs(dataBytes);
                        else
                            SaveToFile(dataBytes, hasBackup);

                        result = true;

                        OnSave?.Invoke(dataJson);
                    }
                }
            }
            catch (Exception ex)
            {
                NLogger.LogException(ex, I);
            }
            return result;
        }

        public static void Load(bool notification = true)
        {
            Dictionary<string, string> loadDictionary = null;
            try
            {
                byte[] data = null;

                if (DeviceHelper.IsWebGL)
                    LoadFromPlayerPrefs(ref data);
                else
                    LoadFromFile(ref data);

                loadDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(data == null ? "{}" : System.Text.Encoding.UTF8.GetString(data, 0, data.Length));
            }
            catch (Exception ex)
            {
                NLogger.LogException(ex, I);
                loadDictionary = null;
            }

            foreach (string key in _saveableDict.Keys)
                _saveableDict[key].SetData(loadDictionary != null && loadDictionary.ContainsKey(key) && loadDictionary[key] != null ? loadDictionary[key] : "");

            if (notification)
            {
                foreach (string key in _saveableDict.Keys)
                    _saveableDict[key].OnAllDataLoaded();
            }
        }

        public static void Load(string data, bool notification = true)
        {
            Dictionary<string, string> loadDictionary = null;
            try
            {
                loadDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(data ?? "{}");
            }
            catch (Exception ex)
            {
                NLogger.LogException(ex, I);
                loadDictionary = null;
            }

            foreach (string key in _saveableDict.Keys)
                _saveableDict[key].SetData(loadDictionary != null && loadDictionary.ContainsKey(key) && loadDictionary[key] != null ? loadDictionary[key] : "");

            if (notification)
            {
                foreach (string key in _saveableDict.Keys)
                    _saveableDict[key].OnAllDataLoaded();
            }
        }

        private static bool SaveToFile(byte[] data, bool hasBackup = true)
        {
            try
            {
                var saveFolderPath = PathHelper.GetSaveFolderPath();
                var savePath = saveFolderPath + $"/{SAVE_NAME}";
                if (hasBackup)
                {
                    var backupSavePath = saveFolderPath + $"/{BACKUP_SAVE_NAME}";
                    if (File.Exists(backupSavePath))
                        File.Delete(backupSavePath);

                    if (File.Exists(savePath))
                        File.Move(savePath, backupSavePath);
                }

                SimpleEncrypt(ref data);
                File.WriteAllBytes(savePath, data);
            }
            catch (Exception e)
            {
                NLogger.LogException(e, I);
                return false;
            }
            return true;
        }

        private static bool SaveToPlayerPrefs(byte[] data)
        {
            try
            {
                SimpleEncrypt(ref data);
                PlayerPrefs.SetString(SAVE_NAME, Convert.ToBase64String(data));
            }
            catch (Exception e)
            {
                NLogger.LogError(e.Message, I);
                return false;
            }
            return true;
        }

        private static bool LoadFromFile(ref byte[] data)
        {
            try
            {
                var saveFolderPath = PathHelper.GetSaveFolderPath();
                var savePath = saveFolderPath + $"/{SAVE_NAME}";
                var backupSavePath = saveFolderPath + $"/{BACKUP_SAVE_NAME}";
                if (File.Exists(savePath))
                    data = File.ReadAllBytes(savePath);
                else if (File.Exists(backupSavePath))
                    data = File.ReadAllBytes(backupSavePath);
                else
                    return false;

                SimpleEncrypt(ref data);
            }
            catch (Exception e)
            {
                NLogger.LogException(e, I);
                return false;
            }
            return true;
        }

        private static bool LoadFromPlayerPrefs(ref byte[] data)
        {
            try
            {
                var save_data = PlayerPrefs.GetString(SAVE_NAME, null);
                if (!string.IsNullOrEmpty(save_data))
                    data = Convert.FromBase64String(save_data);
                else
                    return false;

                SimpleEncrypt(ref data);
            }
            catch (Exception e)
            {
                NLogger.LogException(e, I);
                return false;
            }
            return true;
        }

        public static void DeleteSave()
        {
            _saveableDict.Clear();
            try
            {
                var saveFolderPath = PathHelper.GetSaveFolderPath();
                var savePath = saveFolderPath + $"/{SAVE_NAME}";
                var backupSavePath = saveFolderPath + $"/{BACKUP_SAVE_NAME}";
                if (File.Exists(savePath))
                    File.Delete(savePath);

                if (File.Exists(backupSavePath))
                    File.Delete(backupSavePath);

                NLogger.Log("Deleted save!");
            }
            catch (Exception e)
            {
                NLogger.LogError(e.Message);
            }
        }
        
        //simple encrypt using UDID/decrypt
        private static void SimpleEncrypt(ref byte[] data)
        {
            if (Application.isEditor)
                return;

            byte[] key = System.Text.Encoding.UTF8.GetBytes(SystemInfo.deviceUniqueIdentifier);
            int k_len = key.Length;
            for (uint i = 0; i < data.Length; i++)
                data[i] ^= key[i % k_len];
        }
    }
}