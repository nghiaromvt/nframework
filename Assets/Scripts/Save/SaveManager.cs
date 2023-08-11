using RotaryHeart.Lib.SerializableDictionary;
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

    public class SaveManager : SingletonMono<SaveManager>
    {
#if UNITY_EDITOR
        [UnityEditor.MenuItem("NFramework/Delete Save")]
        public static void DeleteSave()
        {
            _saveDict.Clear();
            try
            {
                var saveFolderPath = PathUtils.GetSaveFolderPath();
                var savePath = saveFolderPath + $"/{SAVE_NAME}";
                var backupSavePath = saveFolderPath + $"/{BACKUP_SAVE_NAME}";
                if (File.Exists(savePath))
                    File.Delete(savePath);

                if (File.Exists(backupSavePath))
                    File.Delete(backupSavePath);

                Logger.Log("Deleted save!");
            }
            catch (Exception e)
            {
                Logger.LogError(e.Message);
            }
        }
#endif

        [Serializable]
        public class LoadDictionary : SerializableDictionaryBase<string, string> { }

        private const string SAVE_NAME = "mwovjtpamcjaytifnhyqlbprths";
        private const string BACKUP_SAVE_NAME = "_" + SAVE_NAME;

        private static Dictionary<string, ISaveable> _saveDict = new Dictionary<string, ISaveable>();

        [SerializeField] private bool _autoSave = true;
        [ConditionalField(nameof(_autoSave))]
        [SerializeField] private float _autoSaveInterval = 5f;

        private bool _needSaveInterupt = true;

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
                if (!_needSaveInterupt)
                    _needSaveInterupt = true;
            }
            else
            {
                if (_needSaveInterupt)
                {
                    _needSaveInterupt = false;
                    Save();
                }
            }
        }

        private void OnApplicationPause(bool pause)
        {
            if (pause)
            {
                if (_needSaveInterupt)
                {
                    _needSaveInterupt = false;
                    Save();
                }
            }
            else
            {
                if (!_needSaveInterupt)
                    _needSaveInterupt = true;
            }
        }

        private void OnApplicationQuit() => Save();

        public void RegisterSaveData(ISaveable data) => _saveDict[data.SaveKey] = data;

        public bool Save(bool hasBackup = true)
        {
            bool result = false;
            try
            {
                bool hasChanged = false;
                foreach (string key in _saveDict.Keys)
                {
                    hasChanged = _saveDict[key].DataChanged;
                    if (hasChanged)
                        break;
                }

                if (hasChanged)
                {
                    LoadDictionary temp = new LoadDictionary();
                    bool checkValid = false;
                    foreach (string key in _saveDict.Keys)
                    {
                        temp[key] = JsonUtility.ToJson(_saveDict[key].GetData());
                        checkValid = true;
                        _saveDict[key].DataChanged = false;
                    }

                    if (checkValid)
                    {
                        byte[] data = System.Text.Encoding.UTF8.GetBytes(JsonUtility.ToJson(temp));

                        if (DeviceInfo.IsWebGL)
                            SaveToPlayerPrefs(data);
                        else
                            SaveToFile(data, hasBackup);

                        result = true;
                    }
                }
            }
            catch (Exception) { }
            return result;
        }

        public void Load(bool notification = true)
        {
            LoadDictionary loadDictionary = null;
            try
            {
                byte[] data = null;

                if (DeviceInfo.IsWebGL)
                    LoadFromPlayerPrefs(ref data);
                else
                    LoadFromFile(ref data);

                loadDictionary = JsonUtility.FromJson<LoadDictionary>(data == null ? "{}" : System.Text.Encoding.UTF8.GetString(data, 0, data.Length));
            }
            catch (Exception)
            {
                loadDictionary = null;
            }

            foreach (string key in _saveDict.Keys)
                _saveDict[key].SetData(loadDictionary != null && loadDictionary.ContainsKey(key) && loadDictionary[key] != null ? loadDictionary[key] : "");

            if (notification)
            {
                foreach (string key in _saveDict.Keys)
                    _saveDict[key].OnAllDataLoaded();
            }
        }

        private bool SaveToFile(byte[] data, bool hasBackup = true)
        {
            try
            {
                var saveFolderPath = PathUtils.GetSaveFolderPath();
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
                Logger.LogError(e.Message, this);
                return false;
            }
            return true;
        }

        private bool SaveToPlayerPrefs(byte[] data)
        {
            try
            {
                SimpleEncrypt(ref data);
                PlayerPrefs.SetString(SAVE_NAME, Convert.ToBase64String(data));
            }
            catch (Exception e)
            {
                Logger.LogError(e.Message);
                return false;
            }
            return true;
        }

        private bool LoadFromFile(ref byte[] data)
        {
            try
            {
                var saveFolderPath = PathUtils.GetSaveFolderPath();
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
                Logger.LogError(e.Message, this);
                return false;
            }
            return true;
        }

        private bool LoadFromPlayerPrefs(ref byte[] data)
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
                Logger.LogError(e.Message, this);
                return false;
            }
            return true;
        }

        //simple encrypt using UDID/decrypt
        private void SimpleEncrypt(ref byte[] data)
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