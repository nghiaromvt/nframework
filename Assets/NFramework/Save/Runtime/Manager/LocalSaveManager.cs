using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace NFramework.Save
{
    public class LocalSaveManager : ISaveService, IApplicationFocusListener, IApplicationPauseListener, IApplicationQuitListener, IDisposable
    {
        private const string SAVE_FILE_NAME = "gamedata.sav";
        private const string ENCRYPTION_KEY = "NFramework_Secret_Save_Key_2026";

        private readonly bool _autoSaveOnQuit;
        private readonly bool _autoSaveOnInterrupt;

        private readonly List<ISaveable> _registeredSaveables = new();
        private readonly Dictionary<string, SaveEntry> _loadedEntries = new();

        private ISerializer _serializer;
        private IEncryptor _encryptor;
        private IChecksumGenerator _checksumGenerator;
        private IStorageService _storageService;
        private bool _isSaving;
        private bool _needSaveInterrupt = true;

        public event Action OnBeforeSave;
        public event Action OnAfterSave;
        public event Action OnBeforeLoad;
        public event Action OnAfterLoad;

        public IReadOnlyList<ISaveable> RegisteredSaveables => _registeredSaveables;
        public IReadOnlyDictionary<string, SaveEntry> LoadedEntries => _loadedEntries;

        public LocalSaveManager(
            bool autoLoad = true,
            bool autoSaveOnQuit = true,
            bool autoSaveOnInterrupt = true,
            ISerializer serializer = null,
            IEncryptor encryptor = null,
            IChecksumGenerator checksumGenerator = null,
            IStorageService storageService = null)
        {
            _autoSaveOnQuit = autoSaveOnQuit;
            _autoSaveOnInterrupt = autoSaveOnInterrupt;

            if (serializer != null && storageService != null)
            {
                InitializeServices(serializer, encryptor, checksumGenerator, storageService);
            }
            else
            {
                InitializeDefaultServices();
            }

            ApplicationLifecycle.Register(this);

            if (autoLoad)
            {
                Load();
            }
        }

        public void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus)
            {
                if (!_needSaveInterrupt)
                {
                    _needSaveInterrupt = true;
                }
            }
            else
            {
                if (_autoSaveOnInterrupt && _needSaveInterrupt)
                {
                    _needSaveInterrupt = false;
                    Save();
                }
            }
        }

        public void OnApplicationPause(bool isPaused)
        {
            if (isPaused)
            {
                if (_autoSaveOnInterrupt && _needSaveInterrupt)
                {
                    _needSaveInterrupt = false;
                    Save();
                }
            }
            else
            {
                if (!_needSaveInterrupt)
                {
                    _needSaveInterrupt = true;
                }
            }
        }

        public void OnApplicationQuit()
        {
            if (_autoSaveOnQuit)
            {
                Save();
            }
        }

        public void Dispose()
        {
            if (ApplicationLifecycle.HasInstance)
            {
                ApplicationLifecycle.Unregister(this);
            }

            if (ServiceLocator.TryGet(out ISaveService currentService) && ReferenceEquals(currentService, this))
            {
                ServiceLocator.Unregister<ISaveService>();
            }
        }

        public void InitializeServices(
            ISerializer serializer,
            IEncryptor encryptor,
            IChecksumGenerator checksumGenerator,
            IStorageService storageService)
        {
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _encryptor = encryptor;
            _checksumGenerator = checksumGenerator;
            _storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));
        }

        public void Register(ISaveable saveable)
        {
            if (saveable == null)
            {
                NLogger.LogError("Cannot register null ISaveable.");
                return;
            }

            if (!_registeredSaveables.Contains(saveable))
            {
                _registeredSaveables.Add(saveable);
            }

            if (_loadedEntries.TryGetValue(saveable.SaveKey, out SaveEntry entry))
            {
                RestoreSaveable(saveable, entry);
            }
            else
            {
                RestoreDefault(saveable);
            }
        }

        public void UnRegister(ISaveable saveable)
        {
            if (saveable != null)
            {
                _registeredSaveables.Remove(saveable);
            }
        }

        public void Save()
        {
            if (_isSaving)
            {
                NLogger.LogError("Save already in progress, skipping concurrent save request.");
                return;
            }

            OnBeforeSave?.Invoke();

            bool anyDirty = CaptureDirtyEntries();
            if (!anyDirty)
            {
                return;
            }

            try
            {
                byte[] rawPayload = BuildPayloadBytes();
                byte[] finalBytes = ProcessOutgoingBytes(rawPayload);

                _storageService.WriteAtomic(SAVE_FILE_NAME, finalBytes);
                OnAfterSave?.Invoke();
            }
            catch (Exception ex)
            {
                NLogger.LogError($"Save failed: {ex.Message}");
            }
        }

        public async UniTask SaveAsync()
        {
            if (_isSaving)
            {
                NLogger.LogError("SaveAsync already in progress, skipping concurrent save request.");
                return;
            }

            _isSaving = true;
            OnBeforeSave?.Invoke();

            try
            {
                bool anyDirty = CaptureDirtyEntries();
                if (!anyDirty)
                {
                    return;
                }

                byte[] rawPayload = BuildPayloadBytes();

                await UniTask.RunOnThreadPool(() =>
                {
                    byte[] finalBytes = ProcessOutgoingBytes(rawPayload);
                    _storageService.WriteAtomic(SAVE_FILE_NAME, finalBytes);
                });

                OnAfterSave?.Invoke();
            }
            catch (Exception ex)
            {
                NLogger.LogError($"SaveAsync failed: {ex.Message}");
            }
            finally
            {
                _isSaving = false;
            }
        }

        public void Load()
        {
            OnBeforeLoad?.Invoke();

            byte[] fileBytes = TryReadFileBytes();
            if (fileBytes == null)
            {
                RestoreDefaultAll();
                OnAfterLoad?.Invoke();
                return;
            }

            try
            {
                UnpackAndRestore(fileBytes);
                OnAfterLoad?.Invoke();
            }
            catch (Exception ex)
            {
                NLogger.LogError($"Failed to unpack save data: {ex.Message}");
                TryRestoreFromBackup();
            }
        }

        public async UniTask LoadAsync()
        {
            OnBeforeLoad?.Invoke();

            byte[] fileBytes = await TryReadFileBytesAsync();
            if (fileBytes == null)
            {
                RestoreDefaultAll();
                OnAfterLoad?.Invoke();
                return;
            }

            try
            {
                SaveFileData fileData = await UniTask.RunOnThreadPool(() => UnpackPayload(fileBytes));
                ApplyLoadedData(fileData);
                OnAfterLoad?.Invoke();
            }
            catch (Exception ex)
            {
                NLogger.LogError($"Failed to async unpack save data: {ex.Message}");
                TryRestoreFromBackup();
            }
        }

        public void DeleteSave()
        {
            _storageService.Delete(SAVE_FILE_NAME);
            _loadedEntries.Clear();
        }

        private void InitializeDefaultServices()
        {
            _serializer = new OdinSerializerHandler();
            _checksumGenerator = new Sha256ChecksumGenerator();

            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                _storageService = new PlayerPrefsStorageService();
                _encryptor = null;
            }
            else
            {
                string saveFolder = PathHelper.GetSaveFolderPath();
                _storageService = new FileStorageService(saveFolder);
                _encryptor = new AesEncryptor(ENCRYPTION_KEY);
            }
        }

        private bool CaptureDirtyEntries()
        {
            bool anyDirty = false;
            foreach (var saveable in _registeredSaveables)
            {
                if (saveable.IsDirty)
                {
                    byte[] stateData = saveable.CaptureState(_serializer);
                    _loadedEntries[saveable.SaveKey] = new SaveEntry(saveable.SaveKey, saveable.SchemaVersion, stateData);
                    saveable.SetClean();
                    anyDirty = true;
                }
            }

            return anyDirty;
        }

        private byte[] BuildPayloadBytes()
        {
            SaveFileData fileData = new SaveFileData();
            foreach (KeyValuePair<string, SaveEntry> pair in _loadedEntries)
            {
                fileData.Entries[pair.Key] = pair.Value;
            }

            return _serializer.Serialize(fileData);
        }

        private byte[] ProcessOutgoingBytes(byte[] payload)
        {
            byte[] processedPayload = _encryptor != null ? _encryptor.Encrypt(payload) : payload;
            byte[] checksum = _checksumGenerator != null
                ? _checksumGenerator.ComputeHash(processedPayload)
                : new byte[SaveFileHeader.CHECKSUM_SIZE];

            SaveFileHeader header = new SaveFileHeader(checksum, SaveFileHeader.CURRENT_FORMAT_VERSION);
            byte[] headerBytes = header.ToBytes();

            byte[] fullBytes = new byte[headerBytes.Length + processedPayload.Length];
            Array.Copy(headerBytes, 0, fullBytes, 0, headerBytes.Length);
            Array.Copy(processedPayload, 0, fullBytes, headerBytes.Length, processedPayload.Length);
            return fullBytes;
        }

        private byte[] TryReadFileBytes()
        {
            if (_storageService.Exists(SAVE_FILE_NAME))
            {
                return _storageService.Read(SAVE_FILE_NAME);
            }

            if (_storageService.HasBackup(SAVE_FILE_NAME))
            {
                return _storageService.ReadBackup(SAVE_FILE_NAME);
            }

            return null;
        }

        private async UniTask<byte[]> TryReadFileBytesAsync()
        {
            if (_storageService.Exists(SAVE_FILE_NAME))
            {
                return await _storageService.ReadAsync(SAVE_FILE_NAME);
            }

            if (_storageService.HasBackup(SAVE_FILE_NAME))
            {
                return await _storageService.ReadBackupAsync(SAVE_FILE_NAME);
            }

            return null;
        }

        private void UnpackAndRestore(byte[] fileBytes)
        {
            SaveFileData fileData = UnpackPayload(fileBytes);
            ApplyLoadedData(fileData);
        }

        private SaveFileData UnpackPayload(byte[] fileBytes)
        {
            if (!SaveFileHeader.TryRead(fileBytes, out SaveFileHeader header, out int payloadOffset))
            {
                throw new InvalidOperationException("Invalid save file header or magic bytes.");
            }

            int payloadLength = fileBytes.Length - payloadOffset;
            byte[] payload = new byte[payloadLength];
            Array.Copy(fileBytes, payloadOffset, payload, 0, payloadLength);

            if (_checksumGenerator != null && !_checksumGenerator.VerifyHash(payload, header.Checksum))
            {
                throw new CryptographicException("Save data checksum verification failed (corrupted file).");
            }

            byte[] decryptedPayload = _encryptor != null ? _encryptor.Decrypt(payload) : payload;
            return _serializer.Deserialize<SaveFileData>(decryptedPayload);
        }

        private void ApplyLoadedData(SaveFileData fileData)
        {
            _loadedEntries.Clear();
            if (fileData?.Entries != null)
            {
                foreach (KeyValuePair<string, SaveEntry> pair in fileData.Entries)
                {
                    _loadedEntries[pair.Key] = pair.Value;
                }
            }

            for (int i = 0; i < _registeredSaveables.Count; i++)
            {
                ISaveable saveable = _registeredSaveables[i];
                if (_loadedEntries.TryGetValue(saveable.SaveKey, out SaveEntry entry))
                {
                    RestoreSaveable(saveable, entry);
                }
                else
                {
                    RestoreDefault(saveable);
                }
            }
        }

        private void TryRestoreFromBackup()
        {
            if (_storageService.HasBackup(SAVE_FILE_NAME))
            {
                try
                {
                    byte[] backupBytes = _storageService.ReadBackup(SAVE_FILE_NAME);
                    UnpackAndRestore(backupBytes);
                    OnAfterLoad?.Invoke();
                }
                catch (Exception ex)
                {
                    NLogger.LogError($"Backup restoration failed: {ex.Message}");
                    RestoreDefaultAll();
                    OnAfterLoad?.Invoke();
                }
            }
            else
            {
                RestoreDefaultAll();
                OnAfterLoad?.Invoke();
            }
        }

        private void RestoreSaveable(ISaveable saveable, SaveEntry entry)
        {
            try
            {
                byte[] data = SchemaMigrationRunner.Migrate(saveable, entry, _serializer);
                saveable.RestoreState(_serializer, data);
                saveable.SetClean();
            }
            catch (Exception ex)
            {
                NLogger.LogError($"Failed to restore state for key '{saveable.SaveKey}': {ex.Message}");
            }
        }

        private void RestoreDefault(ISaveable saveable)
        {
            try
            {
                saveable.RestoreState(_serializer, null);
                saveable.SetClean();
            }
            catch (Exception ex)
            {
                NLogger.LogError($"Failed to restore default state for key '{saveable.SaveKey}': {ex.Message}");
            }
        }

        private void RestoreDefaultAll()
        {
            _loadedEntries.Clear();
            for (int i = 0; i < _registeredSaveables.Count; i++)
            {
                RestoreDefault(_registeredSaveables[i]);
            }
        }
    }
}
