using System;
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

        [ButtonMethod]
        public void MatchWithClipName()
        {
#if UNITY_EDITOR
            if (clip == null)
                return;

            identifier = clip.name;
            var path = UnityEditor.AssetDatabase.GetAssetPath(this);
            UnityEditor.AssetDatabase.RenameAsset(path, $"{identifier}");
#endif
        }
    }
}

