using System;
using System.Collections.Generic;
using UnityEngine;

namespace NFramework
{
    public static class PauseGameHandler
    {
        public static event Action<bool> OnIsPausedChanged; 
        
        private static readonly List<object> _registers = new();
        private static float _cachedTimeScale;
        private static bool _isPaused;

        public static bool IsPaused
        {
            get => _isPaused;
            private set
            {
                if (_isPaused == value) return;
                _isPaused = value;
                OnIsPausedChanged?.Invoke(value);
            }
        }

        public static void Pause(object register)
        {
            NLogger.Log($"Pause by: {register}");
            _registers.Add(register);
            HandleLogic();
        }

        public static void Unpause(object register, bool resetTimeScaleToDefault = false)
        {
            if (!_registers.Contains(register))
            {
                NLogger.LogError($"PauseGameHandler: Not found register: {register}");
                return;
            }
            
            NLogger.Log($"Unpause by: {register}");
            _registers.Remove(register);
            
            if (resetTimeScaleToDefault)
                _cachedTimeScale = 1f;
            
            HandleLogic();
        }

        public static void ForceUnpause(bool resetTimeScaleToDefault = false)
        {
            NLogger.Log($"ForceUnpause");
            _registers.Clear();

            if (resetTimeScaleToDefault)
                _cachedTimeScale = 1f;
            
            HandleLogic();
        }

        private static void HandleLogic()
        {
            if (_registers.Count > 0)
            {
                if (!IsPaused)
                {
                    IsPaused = true;
                    _cachedTimeScale = Time.timeScale;
                    Time.timeScale = 0f;
                    SoundManager.Pause();
                }
            }
            else
            {
                if (IsPaused)
                {
                    IsPaused = false;
                    Time.timeScale = _cachedTimeScale;
                    SoundManager.Unpause();
                }
            }
        }
    }
}
