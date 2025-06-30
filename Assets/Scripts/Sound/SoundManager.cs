using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using PrimeTween;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Audio;
using Random = UnityEngine.Random;

namespace NFramework
{
    public enum EAudioOverlapType
    {
        None = 0, // No handle, audio can play overlap
        StopPrevious = 1, // Stop previous audios
        Skip = 2, // Skip if any audio is playing
    }

    public class SoundManager : SingletonMono<SoundManager>, ISaveable
    {
        private const string BGM_VOLUME_KEY = "BgmVolume";
        private const string BGM_CHILD_VOLUME_KEY = "BgmChildVolume";
        private const string SFX_VOLUME_KEY = "SfxVolume";
        private const string SFX_CHILD_VOLUME_KEY = "SfxChildVolume";

        public static event Action<bool> OnBgmStatusChanged;
        public static event Action<bool> OnSfxStatusChanged;

        [SerializeField] private SaveData _saveData;
        [SerializeField] private int _soundEmitterCount = 10;
        [SerializeField] private AudioMixer _audioMixer;
        [SerializeField] private AudioMixerGroup _bgmMixerGroup;
        [SerializeField] private AudioMixerGroup _sfxMixerGroup;
        [SerializeField] private bool _isLog = true;
        [SerializeField] private bool _initializeOnAwake = true;
        
        private static SoundEmitter _bgmEmitter;
        private static readonly List<SoundEmitter> _allSoundEmitterPool = new();
        private static readonly Queue<SoundEmitter> _soundEmitterPool = new();
        private static readonly List<SoundEmitter> _activeSoundEmitters = new();
        private static readonly Dictionary<string, SoundEmitter> _guidSoundEmitterDict = new();
        private static readonly Dictionary<AudioClip, List<SoundEmitter>> _playingAudioClipDict = new();
        private static Tween _updateBgmMixerTween;
        private static Tween _updateSfxMixerTween;

        [ShowInInspector, ReadOnly, HideInEditorMode] private static readonly Dictionary<string, SoundGroupSO> _cacheSoundGroupResourcesDict = new();
        [ShowInInspector, ReadOnly, HideInEditorMode] private static readonly Dictionary<string, SoundGroupSO> _cacheSoundGroupAddressablesDict = new();
        [ShowInInspector, ReadOnly, HideInEditorMode] private static readonly Dictionary<string, SoundData> _cacheSoundDatas = new();

        #region Status
        
        public static bool BgmStatus
        {
            get => I._saveData.bgmStatus;
            set
            {
                if (!IsInitialized) return;
                
                if (I._saveData.bgmStatus != value)
                {
                    I._saveData.bgmStatus = value;
                    UpdateVolume();
                    OnBgmStatusChanged?.Invoke(value);
                    I.DataChanged = true;
                    Log($"Bgm Status: {value}");
                }
            }
        }

        public static bool SfxStatus
        {
            get => I._saveData.sfxStatus;
            set
            {
                if (!IsInitialized) return;
                
                if (I._saveData.sfxStatus != value)
                {
                    I._saveData.sfxStatus = value;
                    UpdateVolume();
                    OnSfxStatusChanged?.Invoke(value);
                    I.DataChanged = true;
                    Log($"Sfx Status: {value}");
                }
            }
        }
        
        public static bool IsInitialized { get; private set; }
        
        #endregion

        protected override void Awake()
        {
            base.Awake();
            if (_initializeOnAwake) Initialize().Forget();
        }

        public static async UniTask Initialize()
        {
            if (IsInitialized) return;
            I.InitEmitterPool();
            await UniTask.NextFrame();
            SetBgmMixerVolume(1f);
            SetSFXMixerVolume(1f);
            IsInitialized = true;
        }

        public static void Pause() => AudioListener.pause = true;

        public static void Unpause() => AudioListener.pause = false;

        #region Mixer Volume

        public static void SetBgmMixerVolume(float vol, float fadeTime = 0f)
        {
            if (!IsInitialized) return;
            
            vol = Mathf.Clamp(vol, 0.0001f, 1f);
            _updateBgmMixerTween.Stop();
            var targetValue = Mathf.Log10(vol) * 20;
            
            if (fadeTime > 0f)
            {
                I._audioMixer.GetFloat(BGM_CHILD_VOLUME_KEY, out var startValue);
                _updateBgmMixerTween = Tween.Custom(startValue, targetValue, fadeTime, value =>
                {
                    I._audioMixer.SetFloat(BGM_CHILD_VOLUME_KEY, value);
                }, Ease.Linear, useUnscaledTime: true);
            }
            else
            {
                I._audioMixer.SetFloat(BGM_CHILD_VOLUME_KEY, Mathf.Log10(vol) * 20);
            }
        }

        public static void SetSFXMixerVolume(float vol, float fadeTime = 0f)
        {
            if (!IsInitialized) return;
            
            vol = Mathf.Clamp(vol, 0.0001f, 1f);
            _updateSfxMixerTween.Stop();
            var targetValue = Mathf.Log10(vol) * 20;
            
            if (fadeTime > 0f)
            {
                I._audioMixer.GetFloat(SFX_CHILD_VOLUME_KEY, out var startValue);
                _updateBgmMixerTween = Tween.Custom(startValue, targetValue, fadeTime, value =>
                {
                    I._audioMixer.SetFloat(SFX_CHILD_VOLUME_KEY, value);
                }, Ease.Linear, useUnscaledTime: true);
            }
            else
            {
                I._audioMixer.SetFloat(SFX_CHILD_VOLUME_KEY, Mathf.Log10(vol) * 20);
            }
        }

        private static void UpdateVolume()
        {
            SetMixerStatus(BGM_VOLUME_KEY, BgmStatus);
            SetMixerStatus(SFX_VOLUME_KEY, SfxStatus);
        }

        private static void SetMixerStatus(string key, bool status)
        {
            var vol = status ? 1f : 0.0001f;
            I._audioMixer.SetFloat(key, Mathf.Log10(vol) * 20);
        }

        #endregion

        #region Cache/Clear
#if ADDRESSABLES
        public static async UniTask CacheSoundGroupAddressables(string loadKey)
        {
            if (!IsInitialized) return;
            if (_cacheSoundGroupAddressablesDict.ContainsKey(loadKey))
            {
                LogWarning($"Already cache SoundGroup loadKey: {loadKey}");
                return;
            }

            var soundGroupSO = await AddressablesManager.LoadAsset<SoundGroupSO>(loadKey);
            if (!soundGroupSO) return;

            CacheSoundGroup(soundGroupSO);
            _cacheSoundGroupAddressablesDict.Add(loadKey, soundGroupSO);
        }
#endif

        public static async UniTask CacheSoundGroupResources(string loadKey)
        {
            if (!IsInitialized) return;
            if (_cacheSoundGroupResourcesDict.ContainsKey(loadKey))
            {
                LogWarning($"Already cache SoundGroup loadKey: {loadKey}");
                return;
            }
            
            var temp = await Resources.LoadAsync<SoundGroupSO>(loadKey);
            if (temp is not SoundGroupSO soundGroupSO)
            {
                LogError($"CacheSoundResources failed! {loadKey}");
                return;
            }
            
            CacheSoundGroup(soundGroupSO);
            _cacheSoundGroupResourcesDict.Add(loadKey, soundGroupSO);
        }
        
        private static void CacheSoundGroup(SoundGroupSO soundGroupSO)
        {
            foreach (var soundEntry in soundGroupSO.soundEntries)
            {
                if (_cacheSoundDatas.ContainsKey(soundEntry.key))
                {
                    LogWarning($"Already have key in cache: {soundEntry.key}");
                    continue;
                }
                _cacheSoundDatas.Add(soundEntry.key, soundEntry.value);
            }
        }
        
        public static bool ClearSoundGroup(string loadKey)
        {
            SoundGroupSO soundGroupSO = null;
#if ADDRESSABLES
            if (_cacheSoundGroupAddressablesDict.TryGetValue(loadKey, out soundGroupSO))
            {
                ClearSoundGroup(soundGroupSO);
                AddressablesManager.ReleaseAsset(loadKey);
                _cacheSoundGroupAddressablesDict.Remove(loadKey);
                return true;
            }
#endif
            if (_cacheSoundGroupResourcesDict.TryGetValue(loadKey, out soundGroupSO))
            {
                ClearSoundGroup(soundGroupSO);
                _cacheSoundGroupResourcesDict.Remove(loadKey);
                return true;
            }
            
            return false;
        }

        public static void ClearAllSoundGroup()
        {
            foreach (var kv in _cacheSoundGroupAddressablesDict)
            {
                ClearSoundGroup(kv.Key);
            }

            foreach (var kv in _cacheSoundGroupResourcesDict)
            {
                ClearSoundGroup(kv.Key);
            }
            
            _cacheSoundGroupAddressablesDict.Clear();
            _cacheSoundGroupResourcesDict.Clear();
        }
        
        private static void ClearSoundGroup(SoundGroupSO soundGroupSO)
        {
            foreach (var soundEntry in soundGroupSO.soundEntries)
            {
                if (_bgmEmitter.AudioClip == soundEntry.value.clip)
                    _bgmEmitter.Stop();
                
                if (_playingAudioClipDict.TryGetValue(soundEntry.value.clip, out var soundEmitters))
                {
                    var temp = new List<SoundEmitter>(soundEmitters);
                    temp.ForEach(x => x.Stop());
                }
                _cacheSoundDatas.Remove(soundEntry.key);
            }
        }
        
        #endregion

        #region Play

        /// <returns> Guid use to stop sound if needed </returns>
        public static string PlaySfx(string key, float volume = 1f, bool loop = false, float pitch = 1f,
            bool ignorePause = false, EAudioOverlapType audioOverlapType = default, float fadeTime = 0f, Action onStop = null)
        {
            if (_cacheSoundDatas.TryGetValue(key, out var soundData))
            {
                return PlaySfx(soundData.clip, volume * soundData.volumeScale, loop, pitch, ignorePause, audioOverlapType, fadeTime, onStop);
            }
            else
            {
                LogError($"Cannot find SoundData [{key}] in cache");
                return null;
            }
        }

        public static string PlaySfx(AudioClip clip, float volume = 1f, bool loop = false, float pitch = 1f,
            bool ignorePause = false, EAudioOverlapType overlapType = default, float fadeTime = 0f, Action onStop = null)
        {
            if (clip == null)
                return null;

            switch (overlapType)
            {
                case EAudioOverlapType.StopPrevious:
                {
                    if (_playingAudioClipDict.TryGetValue(clip, out var soundEmitters))
                    {
                        var tempSoundEmitters = new List<SoundEmitter>(soundEmitters);
                        tempSoundEmitters.ForEach(em => em.Stop());
                    }

                    break;
                }
                case EAudioOverlapType.Skip:
                {
                    if (_playingAudioClipDict.TryGetValue(clip, out _))
                        return null;

                    break;
                }
            }

            var soundEmitter = GetSoundEmitter();
            if (soundEmitter != null)
            {
                var guid = Guid.NewGuid().ToString();
                soundEmitter.Play(guid, clip, volume, loop, pitch, ignorePause, fadeTime, onStop);

                _activeSoundEmitters.Add(soundEmitter);
                _guidSoundEmitterDict.Add(guid, soundEmitter);

                if (_playingAudioClipDict.TryGetValue(clip, out var soundEmitters))
                    soundEmitters.Add(soundEmitter);
                else
                    _playingAudioClipDict.Add(clip, new List<SoundEmitter> { soundEmitter });

                return guid;
            }

            return null;
        }
        
        public static void PlayBgm(string key, float volume = 1f, bool loop = false, float pitch = 1f,
            bool ignorePause = false, EAudioOverlapType overlapType = default, float fadeTime = 0f, Action onStop = null)
        {
            if (_cacheSoundDatas.TryGetValue(key, out var soundData))
                PlayBgm(soundData.clip, volume * soundData.volumeScale, loop, pitch, ignorePause, overlapType, fadeTime, onStop);
            else
                LogError($"Cannot find AudioClip [{key}] in cache");
        }
        
        public static void PlayBgm(AudioClip clip, float volume = 1f, bool loop = false, float pitch = 1f,
            bool ignorePause = false, EAudioOverlapType overlapType = default, float fadeTime = 0f, Action onStop = null)
        {
            switch (overlapType)
            {
                case EAudioOverlapType.StopPrevious:
                {
                    if (_bgmEmitter.AudioClip == clip)
                        _bgmEmitter.Stop();

                    break;
                }
                case EAudioOverlapType.Skip:
                {
                    if (_bgmEmitter.AudioClip == clip)
                        return;

                    break;
                }
            }
            
            _bgmEmitter.Play("BGM", clip, volume, loop, pitch, ignorePause, fadeTime, onStop);
        }
        
        #endregion

        #region Stop
        
        public static void StopBGM(float fadeTime = 0f)
        {
            if (!_bgmEmitter.enabled) return;
            _bgmEmitter.Stop(fadeTime);
        }

        /// <summary>
        /// Stop a sound by its guid
        /// </summary>
        /// <returns></returns>
        public static bool Stop(string guid)
        {
            if (string.IsNullOrEmpty(guid))
                return false;

            if (_guidSoundEmitterDict.TryGetValue(guid, out var emitter))
            {
                emitter.Stop();
                return true;
            }

            return false;
        }

        public static void StopAll(bool includeBgm = true)
        {
            if (includeBgm)
                StopBGM();

            foreach (var se in _allSoundEmitterPool)
            {
                se.Stop();
            }
        }
        
        #endregion

        /// <summary>
        /// Only call from SoundEmitter
        /// </summary>
        public static void ReturnSoundEmitter(SoundEmitter soundEmitter, AudioClip audioClip)
        {
            if (!_activeSoundEmitters.Contains(soundEmitter))
                return;

            _soundEmitterPool.Enqueue(soundEmitter);
            _activeSoundEmitters.Remove(soundEmitter);
            _guidSoundEmitterDict.Remove(soundEmitter.Guid);

            if (audioClip != null && _playingAudioClipDict.TryGetValue(audioClip, out var soundEmitters))
            {
                soundEmitters.Remove(soundEmitter);
                if (soundEmitters.Count == 0)
                    _playingAudioClipDict.Remove(audioClip);
            }
        }

        private static SoundEmitter GetSoundEmitter()
        {
            if (_soundEmitterPool.Count > 0)
                return _soundEmitterPool.Dequeue();

            LogWarning("Cannot get sound emitter");
            return null;
        }

        private void InitEmitterPool()
        {
            _bgmEmitter = new GameObject("bgmEmitter").AddComponent<SoundEmitter>();
            _bgmEmitter.transform.SetParent(transform);
            _bgmEmitter.SetAudioMixerGroup(_bgmMixerGroup);
            _bgmEmitter.IsBgm = true;

            for (int i = 0; i < _soundEmitterCount; i++)
            {
                var soundEmitterGO = new GameObject("SoundEmitter");
                var soundEmitter = soundEmitterGO.AddComponent<SoundEmitter>();
                soundEmitter.transform.SetParent(transform);
                soundEmitter.SetAudioMixerGroup(_sfxMixerGroup);
                _soundEmitterPool.Enqueue(soundEmitter);
                _allSoundEmitterPool.Add(soundEmitter);
            }
        }

        #region ISaveable
        
        [Serializable]
        public class SaveData
        {
            public bool bgmStatus = true;
            public bool sfxStatus = true;
        }

        public string SaveKey => "SoundManager";

        public bool DataChanged { get; set; }

        public object GetData() => _saveData;

        public void SetData(string data)
        {
            _saveData = string.IsNullOrEmpty(data) ? new SaveData() : JsonUtility.FromJson<SaveData>(data);
            UpdateVolume();
        }

        public void OnAllDataLoaded() { }
        
        #endregion
        
        #region Log

        public static void Log(string message)
        {
            if (I._isLog) NLogger.Log(message, I, Color.blue);
        }

        public static void LogError(string message)
        {
            if (I._isLog) NLogger.LogError(message, I);
        }
        
        public static void LogWarning(string message)
        {
            if (I._isLog) NLogger.LogWarning(message, I);
        }
        
        #endregion
    }
}
