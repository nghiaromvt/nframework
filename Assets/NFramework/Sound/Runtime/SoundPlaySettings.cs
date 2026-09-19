using System;
using UnityEngine;

namespace NFramework.Sound
{
    [Serializable]
    public class SoundPlaySettings
    {
        [Range(0f, 1f)] public float volume = 1f;
        public bool loop;
        public float pitch = 1f;
        public bool ignorePause;
        public AudioOverlapType overlapType;
        public float fadeTime;
    }
}