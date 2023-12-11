using UnityEngine;

namespace NFramework
{
    public class SoundWrapper : MonoBehaviour
    {
        [SerializeField] private SoundSO _soundSO;

        public bool loop;
        public float volumeScale = 1f;
        public float pitchScale = 1f;
        public bool ignoreListnerPause;
        public bool ignoreLisnerVolume;
        public float fadeTime;

        public SoundSO SoundSO => _soundSO;

        public void PlayMusic()
        {
            _soundSO.PlayMusic(loop, volumeScale, pitchScale, ignoreListnerPause, ignoreLisnerVolume, fadeTime);
        }

        public void PlaySFX()
        {
            _soundSO.PlaySFX(loop, volumeScale, pitchScale, ignoreListnerPause, ignoreLisnerVolume, fadeTime);
        }
    }
}

