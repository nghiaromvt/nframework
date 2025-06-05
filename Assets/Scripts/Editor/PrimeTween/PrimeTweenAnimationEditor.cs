using System;
using UnityEditor;
using UnityEngine;

namespace NFramework.Editor
{
    [CustomEditor(typeof(PrimeTweenAnimation))]
    public class PrimeTweenAnimationEditor : UnityEditor.Editor
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
        
        private void OnEnable() 
        {
            _anim = (PrimeTweenAnimation)target;
        }

        private void OnDisable() 
        {
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
            _anim.Play();
        }
        
        private void PlayAllOnGameObject()
        {
            _isPlaying = true;
            _playTargetType = TargetType.AllOnGameObject;
            foreach (var anim in _anim.GetComponents<PrimeTweenAnimation>()) 
            {
                anim.Play();
            }
        }

        private void PlayAllInScene() 
        {
            _isPlaying = true;
            _playTargetType = TargetType.AllInScene;
            foreach (var anim in FindObjectsOfType<PrimeTweenAnimation>()) 
            {
                anim.Play();
            }
        }

        private void Stop()
        {
            _isPlaying = false;
            _anim.Stop();
        }
        
        private void StopAllOnGameObject()
        {
            _isPlaying = false;
            foreach (var anim in _anim.GetComponents<PrimeTweenAnimation>()) 
            {
                anim.Stop();
            }
        }

        private void StopAllInScene() 
        {
            _isPlaying = false;
            foreach (var anim in FindObjectsOfType<PrimeTweenAnimation>()) 
            {
                anim.Stop();
            }
        }
    }
}
