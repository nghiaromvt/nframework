using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using PrimeTween;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Audio;

namespace NFramework.Sound
{
    public enum AudioOverlapType
    {
        None = 0, // No handle, audio can play overlap
        StopPrevious = 1, // Stop previous audios
        Skip = 2, // Skip if any audio is playing
    }

    public class SoundManager : PersistentSingletonMono<SoundManager>
    {
        public const int INVALID_HANDLE = 0;

        private const string BGM_VOLUME_KEY = "BgmVolume";
        private const string BGM_CHILD_VOLUME_KEY = "BgmChildVolume";
        private const string SFX_VOLUME_KEY = "SfxVolume";
        private const string SFX_CHILD_VOLUME_KEY = "SfxChildVolume";
        private const float MIN_VOLUME = 0.0001f;
        private const int BGM_HANDLE = -1;

        public static event Action<bool> OnBgmStatusChanged;
        public static event Action<bool> OnSfxStatusChanged;

        [SerializeField] private SoundEmitter _soundEmitterPrefab;
        [SerializeField] private int _soundEmitterCount = 10;
        [SerializeField] private AudioMixer _audioMixer;
        [SerializeField] private AudioMixerGroup _bgmMixerGroup;
        [SerializeField] private AudioMixerGroup _sfxMixerGroup;
        [SerializeField] private bool _initializeOnAwake = true;

        private SoundEmitter _bgmEmitter;
        // Ordered by play time: index 0 is the oldest, stolen first when the pool runs out.
        private readonly List<SoundEmitter> _activeSoundEmitters = new();
        private readonly Dictionary<int, SoundEmitter> _handleSoundEmitterDict = new();
        private readonly Dictionary<AudioClip, List<SoundEmitter>> _playingAudioClipDict = new();
        private Tween _updateBgmMixerTween;
        private Tween _updateSfxMixerTween;
        private bool _bgmStatus;
        private bool _sfxStatus;
        private int _nextHandle;

        [ShowInInspector, ReadOnly, HideInEditorMode] private readonly Dictionary<string, SoundGroupSO> _cacheSoundGroupResourcesDict = new();
#if ADDRESSABLES
        [ShowInInspector, ReadOnly, HideInEditorMode] private readonly Dictionary<string, AssetHandle<SoundGroupSO>> _cacheSoundGroupAddressablesDict = new();
#endif
        [ShowInInspector, ReadOnly, HideInEditorMode] private readonly Dictionary<string, SoundGroupSO.SoundEntry> _cacheSoundEntries = new();

        #region Status

        /// <summary>
        /// Always applied to the mixer so the first assignment (e.g. from a loaded save) takes effect
        /// even when equal to the default; the event only fires on actual change.
        /// </summary>
        public static bool BgmStatus
        {
            get => I._bgmStatus;
            set
            {
                var changed = I._bgmStatus != value;
                I._bgmStatus = value;
                SetMixerStatus(BGM_VOLUME_KEY, value);
                if (!changed) return;

                OnBgmStatusChanged?.Invoke(value);
                Log($"Bgm Status: {value}");
            }
        }

        /// <inheritdoc cref="BgmStatus"/>
        public static bool SfxStatus
        {
            get => I._sfxStatus;
            set
            {
                var changed = I._sfxStatus != value;
                I._sfxStatus = value;
                SetMixerStatus(SFX_VOLUME_KEY, value);
                if (!changed) return;

                OnSfxStatusChanged?.Invoke(value);
                Log($"Sfx Status: {value}");
            }
        }

        public static bool IsInitialized { get; private set; }

        #endregion

        protected override void Awake()
        {
            base.Awake();
            if (_initializeOnAwake) Initialize().Forget();
        }

        protected override void OnDestroy()
        {
            // A destroyed duplicate must not reset the live instance's static state.
            if (I == this)
            {
                IsInitialized = false;
                OnBgmStatusChanged = null;
                OnSfxStatusChanged = null;
            }
            base.OnDestroy();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatic()
        {
            IsInitialized = false;
            OnBgmStatusChanged = null;
            OnSfxStatusChanged = null;
        }

        public static async UniTask Initialize()
        {
            if (IsInitialized) return;
            I.InitEmitterPool();
            // AudioMixer.SetFloat is ignored during Awake, so wait one frame.
            await UniTask.NextFrame(cancellationToken: I.destroyCancellationToken);
            SetBgmMixerVolume(1f);
            SetSFXMixerVolume(1f);
            IsInitialized = true;
        }

        #region Mixer Volume

        public static void SetBgmMixerVolume(float vol, float fadeTime = 0f) =>
            SetMixerVolume(BGM_CHILD_VOLUME_KEY, ref I._updateBgmMixerTween, vol, fadeTime);

        public static void SetSFXMixerVolume(float vol, float fadeTime = 0f) =>
            SetMixerVolume(SFX_CHILD_VOLUME_KEY, ref I._updateSfxMixerTween, vol, fadeTime);

        private static void SetMixerVolume(string key, ref Tween tween, float vol, float fadeTime)
        {
            tween.Stop();
            var targetValue = ToDecibel(vol);

            if (fadeTime <= 0f)
            {
                I._audioMixer.SetFloat(key, targetValue);
                return;
            }

            I._audioMixer.GetFloat(key, out var startValue);
            tween = Tween.Custom(startValue, targetValue, fadeTime, value => I._audioMixer.SetFloat(key, value),
                Ease.Linear, useUnscaledTime: true);
        }

        private static void SetMixerStatus(string key, bool status) =>
            I._audioMixer.SetFloat(key, ToDecibel(status ? 1f : MIN_VOLUME));

        private static float ToDecibel(float vol) => Mathf.Log10(Mathf.Clamp(vol, MIN_VOLUME, 1f)) * 20f;

        #endregion

        #region Cache/Clear
#if ADDRESSABLES
        public static async UniTask CacheSoundGroupAddressables(string loadKey)
        {
            // Reserve the key before awaiting so concurrent calls with the same key are rejected.
            if (!I._cacheSoundGroupAddressablesDict.TryAdd(loadKey, null))
            {
                LogError($"SoundGroup already cached or loading: {loadKey}");
                return;
            }

            var handle = await AddressablesLoader.LoadAsset<SoundGroupSO>(loadKey);
            if (handle == null || !handle.Result)
            {
                I._cacheSoundGroupAddressablesDict.Remove(loadKey);
                LogError($"CacheSoundGroupAddressables failed! {loadKey}");
                return;
            }

            CacheSoundGroup(handle.Result);
            I._cacheSoundGroupAddressablesDict[loadKey] = handle;
        }
#endif

        public static async UniTask CacheSoundGroupResources(string loadKey)
        {
            if (!I._cacheSoundGroupResourcesDict.TryAdd(loadKey, null))
            {
                LogError($"SoundGroup already cached or loading: {loadKey}");
                return;
            }

            var temp = await Resources.LoadAsync<SoundGroupSO>(loadKey);
            if (temp is not SoundGroupSO soundGroupSO)
            {
                I._cacheSoundGroupResourcesDict.Remove(loadKey);
                LogError($"CacheSoundGroupResources failed! {loadKey}");
                return;
            }

            CacheSoundGroup(soundGroupSO);
            I._cacheSoundGroupResourcesDict[loadKey] = soundGroupSO;
        }

        private static void CacheSoundGroup(SoundGroupSO soundGroupSO)
        {
            foreach (var soundEntry in soundGroupSO.soundEntries)
            {
                if (!I._cacheSoundEntries.TryAdd(soundEntry.key, soundEntry))
                    LogError($"Duplicate sound key in cache: {soundEntry.key} ({soundGroupSO.name})");
            }
        }

        public static bool ClearSoundGroup(string loadKey)
        {
#if ADDRESSABLES
            if (I._cacheSoundGroupAddressablesDict.Remove(loadKey, out var handle))
            {
                // ponytail: clearing a group that is still loading only drops the reservation; the load result is then cached anyway.
                if (handle != null)
                {
                    if (handle.Result) ClearSoundGroup(handle.Result);
                    handle.Dispose();
                }
                return true;
            }
#endif
            if (I._cacheSoundGroupResourcesDict.Remove(loadKey, out var resourcesGroup))
            {
                if (resourcesGroup) ClearSoundGroup(resourcesGroup);
                return true;
            }

            return false;
        }

        public static void ClearAllSoundGroup()
        {
            // Copy keys: ClearSoundGroup removes from the dictionaries being iterated.
#if ADDRESSABLES
            foreach (var key in new List<string>(I._cacheSoundGroupAddressablesDict.Keys))
                ClearSoundGroup(key);
#endif

            foreach (var key in new List<string>(I._cacheSoundGroupResourcesDict.Keys))
                ClearSoundGroup(key);
        }

        private static void ClearSoundGroup(SoundGroupSO soundGroupSO)
        {
            foreach (var soundEntry in soundGroupSO.soundEntries)
            {
                if (I._bgmEmitter != null && I._bgmEmitter.AudioClip == soundEntry.clip)
                    I._bgmEmitter.Stop();

                StopAllOfClip(soundEntry.clip);
                I._cacheSoundEntries.Remove(soundEntry.key);
            }
        }

        #endregion

        #region Play

        /// <returns> Handle used to stop the sound, or <see cref="INVALID_HANDLE"/> </returns>
        public static int PlaySfx(string key, Action onStop = null)
        {
            if (I._cacheSoundEntries.TryGetValue(key, out var soundEntry))
                return PlaySfx(soundEntry.clip, soundEntry.playSettings, onStop);

            LogError($"Cannot find SoundData [{key}] in cache");
            return INVALID_HANDLE;
        }

        /// <returns> Handle used to stop the sound, or <see cref="INVALID_HANDLE"/> </returns>
        public static int PlaySfx(string key, SoundPlaySettings playSettings, Action onStop = null)
        {
            if (I._cacheSoundEntries.TryGetValue(key, out var soundEntry))
                return PlaySfx(soundEntry.clip, playSettings, onStop);

            LogError($"Cannot find SoundData [{key}] in cache");
            return INVALID_HANDLE;
        }

        private static int PlaySfx(AudioClip clip, SoundPlaySettings playSettings, Action onStop = null)
        {
            if (clip == null || playSettings == null)
            {
                LogError($"PlaySfx invalid args: clip={clip}, playSettings={playSettings}");
                return INVALID_HANDLE;
            }

            switch (playSettings.overlapType)
            {
                case AudioOverlapType.StopPrevious:
                    StopAllOfClip(clip);
                    break;
                case AudioOverlapType.Skip:
                    if (I._playingAudioClipDict.ContainsKey(clip))
                        return INVALID_HANDLE;
                    break;
            }

            var soundEmitter = GetSoundEmitter();
            var handle = ++I._nextHandle;
            soundEmitter.Play(handle, clip, playSettings, onStop);

            I._activeSoundEmitters.Add(soundEmitter);
            I._handleSoundEmitterDict.Add(handle, soundEmitter);

            if (I._playingAudioClipDict.TryGetValue(clip, out var soundEmitters))
                soundEmitters.Add(soundEmitter);
            else
                I._playingAudioClipDict.Add(clip, new List<SoundEmitter> { soundEmitter });

            return handle;
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

        private static void PlayBgm(AudioClip clip, SoundPlaySettings playSettings, Action onStop = null)
        {
            StopBGM();

            var bgmEmitter = PoolService.Spawn(I._soundEmitterPrefab, I.transform);
            bgmEmitter.IsBgm = true;
            bgmEmitter.SetAudioMixerGroup(I._bgmMixerGroup);
            I._bgmEmitter = bgmEmitter;

            bgmEmitter.Play(BGM_HANDLE, clip, playSettings, onStop);
        }

        #endregion

        #region Stop

        public static void StopBGM(float fadeTime = 0f)
        {
            if (I._bgmEmitter != null)
            {
                var emitter = I._bgmEmitter;
                I._bgmEmitter = null;
                emitter.Stop(fadeTime);
            }
        }

        public static bool Stop(int handle)
        {
            if (!I._handleSoundEmitterDict.TryGetValue(handle, out var emitter))
                return false;

            emitter.Stop();
            return true;
        }

        public static void StopAll(bool includeBgm = true)
        {
            if (includeBgm)
                StopBGM();

            for (var i = I._activeSoundEmitters.Count - 1; i >= 0; i--)
                I._activeSoundEmitters[i].Stop();
        }

        // Iterate backwards: each Stop() removes the emitter from this same list.
        private static void StopAllOfClip(AudioClip clip)
        {
            if (!I._playingAudioClipDict.TryGetValue(clip, out var soundEmitters))
                return;

            for (var i = soundEmitters.Count - 1; i >= 0; i--)
                soundEmitters[i].Stop();
        }

        #endregion

        /// <summary>
        /// Only call from SoundEmitter
        /// </summary>
        public static void ReturnSoundEmitter(SoundEmitter soundEmitter, AudioClip audioClip)
        {
            if (I._bgmEmitter == soundEmitter)
                I._bgmEmitter = null;

            I._activeSoundEmitters.Remove(soundEmitter);
            I._handleSoundEmitterDict.Remove(soundEmitter.Handle);

            if (I._playingAudioClipDict.TryGetValue(audioClip, out var soundEmitters))
            {
                soundEmitters.Remove(soundEmitter);
                if (soundEmitters.Count == 0)
                    I._playingAudioClipDict.Remove(audioClip);
            }

            PoolService.Despawn(soundEmitter);
        }

        private static SoundEmitter GetSoundEmitter()
        {
            if (I._activeSoundEmitters.Count >= I._soundEmitterCount)
                I._activeSoundEmitters[0].Stop(); // Steal the oldest; Stop() returns it to the pool.

            var soundEmitter = PoolService.Spawn(I._soundEmitterPrefab, I.transform);
            soundEmitter.IsBgm = false;
            soundEmitter.SetAudioMixerGroup(I._sfxMixerGroup);
            return soundEmitter;
        }

        private void InitEmitterPool()
        {
            if (_soundEmitterPrefab == null)
                throw new InvalidOperationException("SoundEmitter prefab is not assigned in SoundManager.");

            // Total pool size accounts for SFX emitter limit + 1 BGM slot
            int totalPoolSize = _soundEmitterCount + 1;
            PoolService.Register(_soundEmitterPrefab, prewarm: totalPoolSize, maxSize: totalPoolSize, scope: PoolScope.Global);
        }

        #region Log

        public static void Log(string message) => NLogger.Log(message, I, Color.blue);

        public static void LogError(string message) => NLogger.LogError(message, I);

        #endregion
    }
}
