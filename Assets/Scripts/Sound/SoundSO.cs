using System;
using System.Collections.Generic;
using UnityEngine;

namespace NFramework
{
    [CreateAssetMenu(menuName = "ScriptableObject/SoundSO")]
    public class SoundSO : ScriptableObject
    {
        public string identifier;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
        [Range(-3f, 3f)] public float pitch = 1f;

        public void Cache() => SoundManager.I.Cache(new List<SoundSO> { this });

        public void ClearCache() => SoundManager.I.ClearCache(new List<SoundSO> { this });

        public void PlayMusic(bool loop = true, float volumeScale = 1f, float pitchScale = 1f,
                  bool ignoreListnerPause = false, bool ignoreLisnerVolume = false, float fadeTime = 0f)
        {
            SoundManager.I.PlayMusic(this, loop, volumeScale, pitchScale,
                ignoreListnerPause, ignoreLisnerVolume, fadeTime);
        }

        public AudioSource PlaySFX(bool loop = false, float volumeScale = 1f, float pitchScale = 1f,
            bool ignoreListnerPause = false, bool ignoreLisnerVolume = false, float fadeTime = 0f)
        {
            return SoundManager.I.PlaySFX(this, loop, volumeScale, pitchScale,
                ignoreListnerPause, ignoreLisnerVolume, fadeTime);
        }
    }
}

