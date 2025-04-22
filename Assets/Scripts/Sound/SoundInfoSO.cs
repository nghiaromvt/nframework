using Sirenix.OdinInspector;
using UnityEngine;

namespace NFramework
{
    [CreateAssetMenu(menuName = "NFramework/Sound/SoundInfo", fileName = "New Sound Info")]
    public class SoundInfoSO : ScriptableObject
    {
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
        public bool loop;
        [HideIf(nameof(randomPitch)), Range(-3f, 3f)] public float pitch = 1f;
        public bool randomPitch;
        [ShowIf(nameof(randomPitch)), Range(-3f, 3f)] public float minRandomPitch = 1f;
        [ShowIf(nameof(randomPitch)), Range(-3f, 3f)] public float maxRandomPitch = 1f;
        public bool ignorePause;
        public EAudioOverlapType overlapType;
        public float fadeTime;

        public string PlaySfx() => SoundManager.PlaySfx(this);

        public void PlayBgm() => SoundManager.PlayBgm(this);
    }
}

