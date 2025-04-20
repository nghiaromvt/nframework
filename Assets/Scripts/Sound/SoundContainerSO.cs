using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace NFramework
{
    [CreateAssetMenu(menuName = "NFramework/SoundContainerSO")]
    public class SoundContainerSO : SerializedScriptableObject
    {
        public Dictionary<string, AudioClip> audioClipDict = new();
        public Dictionary<string, SoundSO> soundSODict = new();
    }
}