using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;
using NFramework.Sound;

namespace NFramework.Sound.Sample
{
    public class SoundSampleTest : MonoBehaviour
    {
        [Title("Custom Settings Override (Optional)")]
        [SerializeField] private SoundPlaySettings _customSettings = new();

        private int _lastSfxHandle = SoundManager.INVALID_HANDLE;
        
        private string SoundGroupKey => SoundDefine.SoundGroupKey.SAMPLE_SOUND_GROUP;
        private string BgmKey => SoundDefine.SoundEntryKey.BGM_INGAME;
        private string SfxKey => SoundDefine.SoundEntryKey.SFX_OPEN_CHEST;

        private void Start()
        {
            // Auto cache sample group on start for quick testing
            CacheGroupFromResources().Forget();
        }

        #region BGM Controls

        [Button(ButtonSizes.Medium), BoxGroup("BGM Controls")]
        public void PlayBgm()
        {
            SoundManager.PlayBgm(BgmKey);
            NLogger.Log($"PlayBgm: {BgmKey}", this);
        }

        [Button(ButtonSizes.Medium), BoxGroup("BGM Controls")]
        public void PlayBgmWithCustomSettings()
        {
            SoundManager.PlayBgm(BgmKey, _customSettings);
            NLogger.Log($"PlayBgm with custom settings: {BgmKey}", this);
        }

        [Button(ButtonSizes.Medium), BoxGroup("BGM Controls")]
        public void StopBgm(float fadeTime = 0.5f)
        {
            SoundManager.StopBGM(fadeTime);
            NLogger.Log($"StopBGM fade: {fadeTime}s", this);
        }

        #endregion

        #region SFX Controls

        [Button(ButtonSizes.Medium), BoxGroup("SFX Controls")]
        public void PlaySfx()
        {
            _lastSfxHandle = SoundManager.PlaySfx(SfxKey);
            NLogger.Log($"PlaySfx [{SfxKey}] handle: {_lastSfxHandle}", this);
        }

        [Button(ButtonSizes.Medium), BoxGroup("SFX Controls")]
        public void PlaySfxWithCustomSettings()
        {
            _lastSfxHandle = SoundManager.PlaySfx(SfxKey, _customSettings);
            NLogger.Log($"PlaySfx [{SfxKey}] with custom settings handle: {_lastSfxHandle}", this);
        }

        [Button(ButtonSizes.Medium), BoxGroup("SFX Controls")]
        public void StopLastSfx()
        {
            if (_lastSfxHandle == SoundManager.INVALID_HANDLE)
            {
                NLogger.LogError("SoundSampleTest: No active SFX handle to stop!", this);
                return;
            }

            var stopped = SoundManager.Stop(_lastSfxHandle);
            NLogger.Log($"Stop handle {_lastSfxHandle} result: {stopped}", this);
            _lastSfxHandle = SoundManager.INVALID_HANDLE;
        }

        [Button(ButtonSizes.Medium), BoxGroup("SFX Controls")]
        public void StopAllSounds()
        {
            SoundManager.StopAll();
            NLogger.Log("StopAll sounds called", this);
        }

        #endregion

        #region Status & Volume

        [Button(ButtonSizes.Medium), BoxGroup("Status & Volume")]
        public void ToggleBgmStatus()
        {
            SoundManager.BgmStatus = !SoundManager.BgmStatus;
            NLogger.Log($"BGM Status now: {SoundManager.BgmStatus}", this);
        }

        [Button(ButtonSizes.Medium), BoxGroup("Status & Volume")]
        public void ToggleSfxStatus()
        {
            SoundManager.SfxStatus = !SoundManager.SfxStatus;
            NLogger.Log($"SFX Status now: {SoundManager.SfxStatus}", this);
        }

        [Button(ButtonSizes.Medium), BoxGroup("Status & Volume")]
        public void SetVolumeBgm(float volume, float fadeTime = 0.5f)
        {
            SoundManager.SetBgmMixerVolume(volume, fadeTime);
            NLogger.Log($"SetBgmMixerVolume: {volume} (fade: {fadeTime}s)", this);
        }

        [Button(ButtonSizes.Medium), BoxGroup("Status & Volume")]
        public void SetVolumeSfx(float volume, float fadeTime = 0.5f)
        {
            SoundManager.SetSFXMixerVolume(volume, fadeTime);
            NLogger.Log($"SetSFXMixerVolume: {volume} (fade: {fadeTime}s)", this);
        }

        #endregion

        #region SoundGroup Cache

        [Button(ButtonSizes.Medium), BoxGroup("SoundGroup Cache")]
        public async UniTask CacheGroupFromResources()
        {
            await SoundManager.CacheSoundGroupResources(SoundGroupKey);
            NLogger.Log($"Cached group: {SoundGroupKey}", this);
        }

        [Button(ButtonSizes.Medium), BoxGroup("SoundGroup Cache")]
        public void ClearGroup()
        {
            var cleared = SoundManager.ClearSoundGroup(SoundGroupKey);
            NLogger.Log($"Cleared group {SoundGroupKey}: {cleared}", this);
        }

        [Button(ButtonSizes.Medium), BoxGroup("SoundGroup Cache")]
        public void ClearAllGroups()
        {
            SoundManager.ClearAllSoundGroup();
            NLogger.Log("Cleared all SoundGroups", this);
        }

        #endregion
    }
}
