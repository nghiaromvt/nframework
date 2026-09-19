using System;
using System.Collections.Generic;
using UnityEngine;

namespace NFramework
{
    [DefaultExecutionOrder(-10000)]
    public class ApplicationLifecycle : LazyPersistentSingletonMono<ApplicationLifecycle>
    {
        private readonly HashSet<object> _registeredObjects = new HashSet<object>();

        private readonly List<IApplicationPauseListener> _pauseListeners = new List<IApplicationPauseListener>(16);
        private readonly List<IApplicationPauseListener> _pendingPauseListeners = new List<IApplicationPauseListener>(8);

        private readonly List<IApplicationFocusListener> _focusListeners = new List<IApplicationFocusListener>(16);
        private readonly List<IApplicationFocusListener> _pendingFocusListeners = new List<IApplicationFocusListener>(8);

        private readonly List<IApplicationQuitListener> _quitListeners = new List<IApplicationQuitListener>(16);
        private readonly List<IApplicationQuitListener> _pendingQuitListeners = new List<IApplicationQuitListener>(8);

        private bool _isPausing;
        private bool _isFocusing;
        private bool _isQuitting;

        private bool _needsPauseCompaction;
        private bool _needsFocusCompaction;
        private bool _needsQuitCompaction;

        private bool _isPaused;
        private bool _isFocused = true;
        private bool _isApplicationQuitting;

        public static bool IsPaused => HasInstance && I._isPaused;
        public static bool IsFocused => !HasInstance || I._isFocused;
        public static bool IsQuitting => HasInstance && I._isApplicationQuitting;

        protected override void Awake()
        {
            base.Awake();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            _isPaused = pauseStatus;
            CommitPendingPauseListeners();

            _isPausing = true;
            for (int i = 0; i < _pauseListeners.Count; i++)
            {
                var item = _pauseListeners[i];
                if (item == null)
                {
                    continue;
                }

                if (item is UnityEngine.Object unityObj && unityObj == null)
                {
                    NLogger.LogError($"Target '{item.GetType().Name}' was destroyed without unregistering from ApplicationLifecycle. Auto-unregistering.");
                    UnregisterInternal(item);
                    continue;
                }

                try
                {
                    item.OnApplicationPause(pauseStatus);
                }
                catch (Exception ex)
                {
                    NLogger.LogException(ex);
                }
            }

            _isPausing = false;

            if (_needsPauseCompaction)
            {
                CompactList(_pauseListeners);
                _needsPauseCompaction = false;
            }
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            _isFocused = hasFocus;
            CommitPendingFocusListeners();

            _isFocusing = true;
            for (int i = 0; i < _focusListeners.Count; i++)
            {
                var item = _focusListeners[i];
                if (item == null)
                {
                    continue;
                }

                if (item is UnityEngine.Object unityObj && unityObj == null)
                {
                    NLogger.LogError($"Target '{item.GetType().Name}' was destroyed without unregistering from ApplicationLifecycle. Auto-unregistering.");
                    UnregisterInternal(item);
                    continue;
                }

                try
                {
                    item.OnApplicationFocus(hasFocus);
                }
                catch (Exception ex)
                {
                    NLogger.LogException(ex);
                }
            }

            _isFocusing = false;

            if (_needsFocusCompaction)
            {
                CompactList(_focusListeners);
                _needsFocusCompaction = false;
            }
        }

        private void OnApplicationQuit()
        {
            _isApplicationQuitting = true;
            CommitPendingQuitListeners();

            _isQuitting = true;
            for (int i = 0; i < _quitListeners.Count; i++)
            {
                var item = _quitListeners[i];
                if (item == null)
                {
                    continue;
                }

                if (item is UnityEngine.Object unityObj && unityObj == null)
                {
                    NLogger.LogError($"Target '{item.GetType().Name}' was destroyed without unregistering from ApplicationLifecycle. Auto-unregistering.");
                    UnregisterInternal(item);
                    continue;
                }

                try
                {
                    item.OnApplicationQuit();
                }
                catch (Exception ex)
                {
                    NLogger.LogException(ex);
                }
            }

            _isQuitting = false;

            if (_needsQuitCompaction)
            {
                CompactList(_quitListeners);
                _needsQuitCompaction = false;
            }
        }

        public static void Register(object target)
        {
            if (target == null)
            {
                NLogger.LogError("Cannot register null target to ApplicationLifecycle.");
                return;
            }

            I.RegisterInternal(target);
        }

        public static void Unregister(object target)
        {
            if (!HasInstance)
            {
                return;
            }

            if (target == null)
            {
                NLogger.LogError("Cannot unregister null target from ApplicationLifecycle.");
                return;
            }

            I.UnregisterInternal(target);
        }

        private void RegisterInternal(object target)
        {
            if (!_registeredObjects.Add(target))
            {
                NLogger.LogError($"Target '{target.GetType().Name}' is already registered in ApplicationLifecycle.");
                return;
            }

            bool implementsAny = false;

            if (target is IApplicationPauseListener pauseListener)
            {
                implementsAny = true;
                if (_isPausing)
                {
                    _pendingPauseListeners.Add(pauseListener);
                }
                else
                {
                    _pauseListeners.Add(pauseListener);
                }
            }

            if (target is IApplicationFocusListener focusListener)
            {
                implementsAny = true;
                if (_isFocusing)
                {
                    _pendingFocusListeners.Add(focusListener);
                }
                else
                {
                    _focusListeners.Add(focusListener);
                }
            }

            if (target is IApplicationQuitListener quitListener)
            {
                implementsAny = true;
                if (_isQuitting)
                {
                    _pendingQuitListeners.Add(quitListener);
                }
                else
                {
                    _quitListeners.Add(quitListener);
                }
            }

            if (!implementsAny)
            {
                _registeredObjects.Remove(target);
                NLogger.LogError($"Target '{target.GetType().Name}' does not implement IApplicationPauseListener, IApplicationFocusListener, or IApplicationQuitListener.");
            }
        }

        private void UnregisterInternal(object target)
        {
            if (!_registeredObjects.Remove(target))
            {
                NLogger.LogError($"Target '{target.GetType().Name}' is not registered in ApplicationLifecycle.");
                return;
            }

            if (target is IApplicationPauseListener pauseListener)
            {
                int pendingIndex = _pendingPauseListeners.IndexOf(pauseListener);
                if (pendingIndex >= 0)
                {
                    _pendingPauseListeners.RemoveAt(pendingIndex);
                }

                int index = _pauseListeners.IndexOf(pauseListener);
                if (index >= 0)
                {
                    _pauseListeners[index] = null;
                    _needsPauseCompaction = true;
                }
            }

            if (target is IApplicationFocusListener focusListener)
            {
                int pendingIndex = _pendingFocusListeners.IndexOf(focusListener);
                if (pendingIndex >= 0)
                {
                    _pendingFocusListeners.RemoveAt(pendingIndex);
                }

                int index = _focusListeners.IndexOf(focusListener);
                if (index >= 0)
                {
                    _focusListeners[index] = null;
                    _needsFocusCompaction = true;
                }
            }

            if (target is IApplicationQuitListener quitListener)
            {
                int pendingIndex = _pendingQuitListeners.IndexOf(quitListener);
                if (pendingIndex >= 0)
                {
                    _pendingQuitListeners.RemoveAt(pendingIndex);
                }

                int index = _quitListeners.IndexOf(quitListener);
                if (index >= 0)
                {
                    _quitListeners[index] = null;
                    _needsQuitCompaction = true;
                }
            }
        }

        private void CommitPendingPauseListeners()
        {
            if (_pendingPauseListeners.Count == 0)
            {
                return;
            }

            for (int i = 0; i < _pendingPauseListeners.Count; i++)
            {
                _pauseListeners.Add(_pendingPauseListeners[i]);
            }
            _pendingPauseListeners.Clear();
        }

        private void CommitPendingFocusListeners()
        {
            if (_pendingFocusListeners.Count == 0)
            {
                return;
            }

            for (int i = 0; i < _pendingFocusListeners.Count; i++)
            {
                _focusListeners.Add(_pendingFocusListeners[i]);
            }
            _pendingFocusListeners.Clear();
        }

        private void CommitPendingQuitListeners()
        {
            if (_pendingQuitListeners.Count == 0)
            {
                return;
            }

            for (int i = 0; i < _pendingQuitListeners.Count; i++)
            {
                _quitListeners.Add(_pendingQuitListeners[i]);
            }
            _pendingQuitListeners.Clear();
        }

        private static void CompactList<T>(List<T> list) where T : class
        {
            int writeIndex = 0;
            for (int readIndex = 0; readIndex < list.Count; readIndex++)
            {
                var element = list[readIndex];
                if (element != null)
                {
                    if (writeIndex != readIndex)
                    {
                        list[writeIndex] = element;
                    }
                    writeIndex++;
                }
            }

            int countToRemove = list.Count - writeIndex;
            if (countToRemove > 0)
            {
                list.RemoveRange(writeIndex, countToRemove);
            }
        }
    }
}
