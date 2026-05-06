using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace NFramework.Editor
{
    [CustomEditor(typeof(PrimeTweenAnimation), true)]
    public class PrimeTweenAnimationEditor : OdinEditor
    {
        public enum TargetType
        {
            Self,
            AllOnGameObject,
            AllInScene
        }
        
        private PrimeTweenAnimation _anim;
        private bool _isPlaying;
        private TargetType _playTargetType;

        protected override void OnEnable()
        {
            base.OnEnable();
            _anim = (PrimeTweenAnimation)target;
        }

        protected override void OnDisable() 
        {
            if (!_anim || Application.isPlaying) return;
            
            switch (_playTargetType)
            {
                default:
                case TargetType.Self:
                    Stop();
                    break;
                case TargetType.AllOnGameObject:
                    StopAllOnGameObject();
                    break;
                case TargetType.AllInScene:
                    StopAllInScene();
                    break;
            }
        }
        
        public override void OnInspectorGUI() 
        {
            if (!_anim) return;
            
            if (!Application.isPlaying) 
            {
                GUILayout.BeginHorizontal();
                
                if (!_isPlaying) 
                {
                    if (GUILayout.Button("Play")) 
                        Play();
                    if (GUILayout.Button("Play All on GameObject"))
                        PlayAllOnGameObject();
                    if (GUILayout.Button("Play All in Scene"))
                        PlayAllInScene();
                } 
                else 
                {
                    if (GUILayout.Button("Stop")) 
                        Stop();
                    if (GUILayout.Button("Stop All on GameObject"))
                        StopAllOnGameObject();
                    if (GUILayout.Button("Stop All in Scene"))
                        StopAllInScene();
                }
                
                GUILayout.EndHorizontal();
            } 
            else 
            {
                EditorGUILayout.HelpBox("Animation Editor disabled while in play mode", MessageType.Info);
            }
            
            GUILayout.Space(10);
            DrawDefaultInspector();
        }

        private void Play()
        {
            _isPlaying = true;
            _playTargetType = TargetType.Self;
            _anim.StartTween();
        }
        
        private void PlayAllOnGameObject()
        {
            _isPlaying = true;
            _playTargetType = TargetType.AllOnGameObject;
            foreach (var anim in _anim.GetComponents<PrimeTweenAnimation>()) 
            {
                anim.StartTween();
            }
        }

        private void PlayAllInScene() 
        {
            _isPlaying = true;
            _playTargetType = TargetType.AllInScene;
            foreach (var anim in FindObjectsOfType<PrimeTweenAnimation>()) 
            {
                anim.StartTween();
            }
        }

        private void Stop()
        {
            _isPlaying = false;
            _anim.StopTweenAndResetValue();
        }
        
        private void StopAllOnGameObject()
        {
            _isPlaying = false;
            foreach (var anim in _anim.GetComponents<PrimeTweenAnimation>()) 
            {
                anim.StopTweenAndResetValue();
            }
        }

        private void StopAllInScene() 
        {
            _isPlaying = false;
            foreach (var anim in FindObjectsOfType<PrimeTweenAnimation>()) 
            {
                anim.StopTweenAndResetValue();
            }
        }
    }
}
