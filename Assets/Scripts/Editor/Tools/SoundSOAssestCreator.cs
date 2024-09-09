using System.IO;
using UnityEditor;
using UnityEngine;

namespace NFramework.Editors
{
    public static class SoundSOAssestCreator
    {
        [MenuItem("Assets/Create SoundSO")]
        public static void Create()
        {
            var audioClip = Selection.activeObject as AudioClip;
            var soundSO = ScriptableObject.CreateInstance(typeof(SoundSO)) as SoundSO;
            soundSO.clip = audioClip;
            
            var path = Path.GetDirectoryName(AssetDatabase.GetAssetPath(audioClip));
            AssetDatabase.CreateAsset(soundSO, $"{path}/{Selection.activeObject.name}.asset");
        }

        [MenuItem("Assets/Create SoundSO", true)]
        public static bool ValidateCreate()
        {
            var audioClip = Selection.activeObject as AudioClip;
            return audioClip != null;
        }
    }
}