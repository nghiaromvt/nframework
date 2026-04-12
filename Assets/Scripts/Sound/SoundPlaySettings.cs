using System;
using UnityEngine;

namespace NFramework
{
    [Serializable]
    public class SoundPlaySettings
    {
        [Range(0f, 1f)] public float volume = 1f;
        public bool loop;
        public float pitch = 1f;
        public bool ignorePause;
        public EAudioOverlapType overlapType;
        public float fadeTime;
    }
}