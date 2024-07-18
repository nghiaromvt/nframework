using UnityEngine;

namespace NFramework
{
    public class SoundWrapper : MonoBehaviour
    {
        [SerializeField] private SoundSO _soundSO;

        public bool isSFX;
        public bool autoPlay;
        [Separator]
        public bool loop;
        public float volumeScale = 1f;
        public float pitchScale = 1f;
        public bool ignoreListnerPause;
        public bool ignoreLisnerVolume;
        public float fadeTime;

        public SoundSO SoundSO => _soundSO;

        private void OnEnable()
        {
            if (autoPlay)
                Play();
        }

        [ButtonMethod]
        public void Play()
        {
            if (isSFX)
                _soundSO.PlayMusic(loop, volumeScale, pitchScale, ignoreListnerPause, ignoreLisnerVolume, fadeTime);
            else
                _soundSO.PlaySFX(loop, volumeScale, pitchScale, ignoreListnerPause, ignoreLisnerVolume, fadeTime);
        }
    }
}

