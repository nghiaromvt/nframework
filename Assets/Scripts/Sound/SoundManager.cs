using System;
using System.Collections.Generic;
using System.Reflection;
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
        private const BindingFlags BINDING_FLAGS = BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic;

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
        private static readonly Dictionary<string, SoundContainerSO> _cacheSoundContainerSOResourcesDict = new();
        private static readonly Dictionary<string, SoundContainerSO> _cacheSoundContainerSOAddressablesDict = new();
        private static Tween _updateBgmMixerTween;
        private static Tween _updateSfxMixerTween;

        [ShowInInspector, ReadOnly, HideInEditorMode] private static readonly Dictionary<string, AudioClip> _cacheAudioClips = new();
        [ShowInInspector, ReadOnly, HideInEditorMode] private static readonly Dictionary<string, SoundSO> _cacheSoundSO = new();

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

        public static async UniTask CacheSoundAddressables(string id)
        {
            if (!IsInitialized) return;
            if (_cacheSoundContainerSOAddressablesDict.ContainsKey(id))
            {
                LogWarning($"Already cache SoundAddressables! {id}");
                return;
            }

            var soundContainerSO = await AddressablesManager.LoadAsset<SoundContainerSO>(id);
            if (!soundContainerSO) return;

            CacheSound(soundContainerSO);
            _cacheSoundContainerSOAddressablesDict.Add(id, soundContainerSO);
        }

        public static async UniTask CacheSoundResources(string id)
        {
            if (!IsInitialized) return;
            if (_cacheSoundContainerSOResourcesDict.ContainsKey(id))
            {
                LogWarning($"Already cache SoundResources! {id}");
                return;
            }
            
            var temp = await Resources.LoadAsync<SoundContainerSO>(id);
            if (temp is not SoundContainerSO soundContainerSO)
            {
                LogError($"CacheSoundResources failed! {id}");
                return;
            }
            
            CacheSound(soundContainerSO);
            _cacheSoundContainerSOResourcesDict.Add(id, soundContainerSO);
        }
        
        public static bool ClearCache(string id)
        {
            if (_cacheSoundContainerSOAddressablesDict.TryGetValue(id, out SoundContainerSO soundContainerSO))
            {
                ClearSound(soundContainerSO);
                AddressablesManager.ReleaseAsset(id);
                return true;
            }
            else if (_cacheSoundContainerSOResourcesDict.TryGetValue(id, out soundContainerSO))
            {
                ClearSound(soundContainerSO);
                return true;
            }
            return false;
        }
        
        private static void CacheSound(SoundContainerSO soundContainerSO)
        {
            foreach (var kv in soundContainerSO.audioClipDict)
            {
                if (_cacheSoundSO.ContainsKey(kv.Key))
                {
                    LogWarning($"Already have key in cacheAudioClips: {kv.Key}");
                    continue;
                }
                
                _cacheAudioClips.Add(kv.Key, kv.Value);
            }

            foreach (var kv in soundContainerSO.soundSODict)
            {
                if (_cacheSoundSO.ContainsKey(kv.Key))
                {
                    LogWarning($"Already have key in cacheSoundSO: {kv.Key}");
                    continue;
                }
                
                _cacheSoundSO.Add(kv.Key, kv.Value);
            }
        }
        
        private static void ClearSound(SoundContainerSO soundContainerSO)
        {
            foreach (var kv in soundContainerSO.audioClipDict)
            {
                if (_bgmEmitter.AudioClip == kv.Value)
                {
                    _bgmEmitter.Stop();
                    continue;
                }
                
                if (_playingAudioClipDict.TryGetValue(kv.Value, out var soundEmitters))
                {
                    var temp = new List<SoundEmitter>(soundEmitters);
                    temp.ForEach(x => x.Stop());
                }
                _cacheAudioClips.Remove(kv.Key);
            }

            foreach (var kv in soundContainerSO.soundSODict)
            {
                if (_bgmEmitter.AudioClip == kv.Value.clip)
                {
                    _bgmEmitter.Stop();
                    continue;
                }
                
                if (_playingAudioClipDict.TryGetValue(kv.Value.clip, out var soundEmitters))
                {
                    var temp = new List<SoundEmitter>(soundEmitters);
                    temp.ForEach(x => x.Stop());
                }
                _cacheSoundSO.Remove(kv.Key);
            }
        }
        
        #endregion

        #region Play

        /// <summary>
        /// Play sound through SoundSO
        /// </summary>
        /// <returns>guid use to stop sound if needed</returns>
        public static string PlaySfx(SoundSO soundSO, Action onStop = null)
        {
            var pitch = soundSO.randomPitch ? Random.Range(soundSO.minRandomPitch, soundSO.maxRandomPitch) : soundSO.pitch;
            return PlaySfx(soundSO.clip, soundSO.volume, soundSO.loop, pitch, soundSO.ignorePause, soundSO.overlapType, soundSO.fadeTime, onStop);
        }
        
        public static string PlaySfxInCacheSoundSO(string address, Action onStop = null)
        {
            if (_cacheSoundSO.TryGetValue(address, out var soundSO))
            {
                return PlaySfx(soundSO, onStop);
            }
            else
            {
                LogError($"Cannot find SoundSO [{address}] in cache");
                return null;
            }
        }
        
        public static string PlaySfxInCacheAudioClip(string address, float volume = 1f, bool loop = false, float pitch = 1f,
            bool ignorePause = false, EAudioOverlapType audioOverlapType = default, float fadeTime = 0f, Action onStop = null)
        {
            if (_cacheAudioClips.TryGetValue(address, out var clip))
            {
                return PlaySfx(clip, volume, loop, pitch, ignorePause, audioOverlapType, fadeTime, onStop);
            }
            else
            {
                LogError($"Cannot find AudioClip [{address}] in cache");
                return null;
            }
        }

        /// <summary>
        /// Play sound directly by an audio clip
        /// </summary>
        /// <returns> Guid use to stop sound if needed </returns>
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
                    if (_playingAudioClipDict.TryGetValue(clip, out var soundEmitters))
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

        public static void PlayBgm(SoundSO soundSO, Action onStop = null)
        {
            var pitch = soundSO.randomPitch ? Random.Range(soundSO.minRandomPitch, soundSO.maxRandomPitch) : soundSO.pitch;
            PlayBgm(soundSO.clip, soundSO.volume, soundSO.loop, pitch, soundSO.ignorePause, soundSO.overlapType, soundSO.fadeTime, onStop);
        }

        public static void PlayBgmInCacheSoundSO(string address, Action onStop = null)
        {
            if (_cacheSoundSO.TryGetValue(address, out var soundSO))
                PlayBgm(soundSO, onStop);
            else
                LogError($"Cannot find SoundSO [{address}] in cache");
        }

        public static void PlayBgmInCacheAudioClip(string address, float volume = 1f, bool loop = false, float pitch = 1f,
            bool ignorePause = false, EAudioOverlapType overlapType = default, float fadeTime = 0f, Action onStop = null)
        {
            if (_cacheAudioClips.TryGetValue(address, out var audioClip))
                PlayBgm(audioClip, volume, loop, pitch, ignorePause, overlapType, fadeTime, onStop);
            else
                LogError($"Cannot find AudioClip [{address}] in cache");
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
