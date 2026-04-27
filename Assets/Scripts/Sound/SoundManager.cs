using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using PrimeTween;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Audio;

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
        [SerializeField] private string _resourcesRootFolder;
        [SerializeField] private string _refPathAddressable;
        
        private SoundEmitter _bgmEmitter;
        private readonly List<SoundEmitter> _allSoundEmitterPool = new();
        private readonly Queue<SoundEmitter> _soundEmitterPool = new();
        private readonly List<SoundEmitter> _activeSoundEmitters = new();
        private readonly Dictionary<string, SoundEmitter> _guidSoundEmitterDict = new();
        private readonly Dictionary<AudioClip, List<SoundEmitter>> _playingAudioClipDict = new();
        private Tween _updateBgmMixerTween;
        private Tween _updateSfxMixerTween;

        [ShowInInspector, ReadOnly, HideInEditorMode] private readonly Dictionary<string, SoundGroupSO> _cacheSoundGroupResourcesDict = new();
        [ShowInInspector, ReadOnly, HideInEditorMode] private readonly Dictionary<string, SoundGroupSO> _cacheSoundGroupAddressablesDict = new();
        [ShowInInspector, ReadOnly, HideInEditorMode] private readonly Dictionary<string, SoundGroupSO.SoundEntry> I._cacheSoundEntries = new();

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
            await UniTask.NextFrame(cancellationToken: I.destroyCancellationToken);
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
            I._updateBgmMixerTween.Stop();
            var targetValue = Mathf.Log10(vol) * 20;
            
            if (fadeTime > 0f)
            {
                I._audioMixer.GetFloat(BGM_CHILD_VOLUME_KEY, out var startValue);
                I._updateBgmMixerTween = Tween.Custom(startValue, targetValue, fadeTime, value =>
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
            I._updateSfxMixerTween.Stop();
            var targetValue = Mathf.Log10(vol) * 20;
            
            if (fadeTime > 0f)
            {
                I._audioMixer.GetFloat(SFX_CHILD_VOLUME_KEY, out var startValue);
                I._updateSfxMixerTween = Tween.Custom(startValue, targetValue, fadeTime, value =>
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
            if (I._cacheSoundGroupAddressablesDict.ContainsKey(loadKey))
            {
                LogWarning($"Already cache SoundGroup loadKey: {loadKey}");
                return;
            }

            var soundGroupSO = await AddressablesManager.LoadAsset<SoundGroupSO>(GetSoundGroupAddressablesPath(loadKey));
            if (!soundGroupSO) return;

            CacheSoundGroup(soundGroupSO);
            I._cacheSoundGroupAddressablesDict.Add(loadKey, soundGroupSO);
        }
        
        private static string GetSoundGroupAddressablesPath(string id) => $"{I._refPathAddressable}/{id}.asset";
#endif

        public static async UniTask CacheSoundGroupResources(string loadKey)
        {
            if (!IsInitialized) return;
            if (I._cacheSoundGroupResourcesDict.ContainsKey(loadKey))
            {
                LogWarning($"Already cache SoundGroup loadKey: {loadKey}");
                return;
            }
            
            var temp = await Resources.LoadAsync<SoundGroupSO>($"{I._resourcesRootFolder}{loadKey}");
            if (temp is not SoundGroupSO soundGroupSO)
            {
                LogError($"CacheSoundResources failed! {loadKey}");
                return;
            }
            
            CacheSoundGroup(soundGroupSO);
            I._cacheSoundGroupResourcesDict.Add(loadKey, soundGroupSO);
        }
        
        private static void CacheSoundGroup(SoundGroupSO soundGroupSO)
        {
            foreach (var soundEntry in soundGroupSO.soundEntries)
            {
                if (I._cacheSoundEntries.ContainsKey(soundEntry.key))
                {
                    LogWarning($"Already have key in cache: {soundEntry.key}");
                    continue;
                }
                I._cacheSoundEntries.Add(soundEntry.key, soundEntry);
            }
        }
        
        public static bool ClearSoundGroup(string loadKey)
        {
            SoundGroupSO soundGroupSO = null;
#if ADDRESSABLES
            if (I._cacheSoundGroupAddressablesDict.TryGetValue(loadKey, out soundGroupSO))
            {
                ClearSoundGroup(soundGroupSO);
                AddressablesManager.ReleaseAsset(GetSoundGroupAddressablesPath(loadKey));
                I._cacheSoundGroupAddressablesDict.Remove(loadKey);
                return true;
            }
#endif
            if (I._cacheSoundGroupResourcesDict.TryGetValue(loadKey, out soundGroupSO))
            {
                ClearSoundGroup(soundGroupSO);
                I._cacheSoundGroupResourcesDict.Remove(loadKey);
                return true;
            }
            
            return false;
        }

        public static void ClearAllSoundGroup()
        {
            foreach (var kv in I._cacheSoundGroupAddressablesDict)
            {
                ClearSoundGroup(kv.Key);
            }

            foreach (var kv in I._cacheSoundGroupResourcesDict)
            {
                ClearSoundGroup(kv.Key);
            }
            
            I._cacheSoundGroupAddressablesDict.Clear();
            I._cacheSoundGroupResourcesDict.Clear();
        }
        
        private static void ClearSoundGroup(SoundGroupSO soundGroupSO)
        {
            foreach (var soundEntry in soundGroupSO.soundEntries)
            {
                if (I._bgmEmitter.AudioClip == soundEntry.clip)
                    I._bgmEmitter.Stop();
                
                if (I._playingAudioClipDict.TryGetValue(soundEntry.clip, out var soundEmitters))
                {
                    var temp = new List<SoundEmitter>(soundEmitters);
                    temp.ForEach(x => x.Stop());
                }
                I._cacheSoundEntries.Remove(soundEntry.key);
            }
        }
        
        #endregion

        #region Play
        
        /// <returns> Guid use to stop sound if needed </returns>
        public static string PlaySfx(string key, Action onStop = null)
        {
            if (I._cacheSoundEntries.TryGetValue(key, out var soundEntry))
            {
                return PlaySfx(soundEntry.clip, soundEntry.playSettings, onStop);
            }
            else
            {
                LogError($"Cannot find SoundData [{key}] in cache");
                return null;
            }
        }

        /// <returns> Guid use to stop sound if needed </returns>
        public static string PlaySfx(string key, SoundPlaySettings playSettings, Action onStop = null)
        {
            if (I._cacheSoundEntries.TryGetValue(key, out var soundEntry))
            {
                return PlaySfx(soundEntry.clip, playSettings, onStop);
            }
            else
            {
                LogError($"Cannot find SoundData [{key}] in cache");
                return null;
            }
        }

        public static string PlaySfx(AudioClip clip, SoundPlaySettings playSettings, Action onStop = null)
        {
            if (clip == null || playSettings == null)
                return null;

            switch (playSettings.overlapType)
            {
                case EAudioOverlapType.StopPrevious:
                {
                    if (I._playingAudioClipDict.TryGetValue(clip, out var soundEmitters))
                    {
                        var tempSoundEmitters = new List<SoundEmitter>(soundEmitters);
                        tempSoundEmitters.ForEach(em => em.Stop());
                    }

                    break;
                }
                case EAudioOverlapType.Skip:
                {
                    if (I._playingAudioClipDict.TryGetValue(clip, out _))
                        return null;

                    break;
                }
            }

            var soundEmitter = GetSoundEmitter();
            if (soundEmitter != null)
            {
                var guid = Guid.NewGuid().ToString();
                soundEmitter.Play(guid, clip, playSettings, onStop);

                I._activeSoundEmitters.Add(soundEmitter);
                I._guidSoundEmitterDict.Add(guid, soundEmitter);

                if (I._playingAudioClipDict.TryGetValue(clip, out var soundEmitters))
                    soundEmitters.Add(soundEmitter);
                else
                    I._playingAudioClipDict.Add(clip, new List<SoundEmitter> { soundEmitter });

                return guid;
            }

            return null;
        }
        
        public static void PlayBgm(string key, Action onStop = null)
        {
            if (I._cacheSoundEntries.TryGetValue(key, out var soundEntry))
                PlayBgm(soundEntry.clip, soundEntry.playSettings, onStop);
            else
                LogError($"Cannot find AudioClip [{key}] in cache");
        }
        
        public static void PlayBgm(string key, SoundPlaySettings playSettings, Action onStop = null)
        {
            if (I._cacheSoundEntries.TryGetValue(key, out var soundEntry))
                PlayBgm(soundEntry.clip, playSettings, onStop);
            else
                LogError($"Cannot find AudioClip [{key}] in cache");
        }
        
        public static void PlayBgm(AudioClip clip, SoundPlaySettings playSettings, Action onStop = null)
        {
            I._bgmEmitter.Stop();
            I._bgmEmitter.Play("BGM", clip, playSettings, onStop);
        }
        
        #endregion

        #region Stop
        
        public static void StopBGM(float fadeTime = 0f)
        {
            if (!I._bgmEmitter.enabled) return;
            I._bgmEmitter.Stop(fadeTime);
        }

        /// <summary>
        /// Stop a sound by its guid
        /// </summary>
        /// <returns></returns>
        public static bool Stop(string guid)
        {
            if (string.IsNullOrEmpty(guid))
                return false;

            if (I._guidSoundEmitterDict.TryGetValue(guid, out var emitter))
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

            foreach (var se in I._allSoundEmitterPool)
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
            if (!I._activeSoundEmitters.Contains(soundEmitter))
                return;

            I._soundEmitterPool.Enqueue(soundEmitter);
            I._activeSoundEmitters.Remove(soundEmitter);
            I._guidSoundEmitterDict.Remove(soundEmitter.Guid);

            if (audioClip != null && I._playingAudioClipDict.TryGetValue(audioClip, out var soundEmitters))
            {
                soundEmitters.Remove(soundEmitter);
                if (soundEmitters.Count == 0)
                    I._playingAudioClipDict.Remove(audioClip);
            }
        }

        private static SoundEmitter GetSoundEmitter()
        {
            if (I._soundEmitterPool.Count > 0)
                return I._soundEmitterPool.Dequeue();

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
