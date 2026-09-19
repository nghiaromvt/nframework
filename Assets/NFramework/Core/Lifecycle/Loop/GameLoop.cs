using System;
using System.Collections.Generic;
using UnityEngine;

namespace NFramework
{
    /// <summary>
    /// Centralized game loop dispatcher that replaces individual MonoBehaviour Update/FixedUpdate/LateUpdate calls
    /// to minimize native-to-managed reflection overhead, isolate exceptions, and allow safe mutations mid-loop.
    /// </summary>
    [DefaultExecutionOrder(-10000)]
    public class GameLoop : LazyPersistentSingletonMono<GameLoop>
    {
        private readonly HashSet<object> _registeredObjects = new HashSet<object>();

        private readonly List<IUpdatable> _updatables = new List<IUpdatable>(64);
        private readonly List<IUpdatable> _pendingUpdatables = new List<IUpdatable>(16);

        private readonly List<IFixedUpdatable> _fixedUpdatables = new List<IFixedUpdatable>(64);
        private readonly List<IFixedUpdatable> _pendingFixedUpdatables = new List<IFixedUpdatable>(16);

        private readonly List<ILateUpdatable> _lateUpdatables = new List<ILateUpdatable>(64);
        private readonly List<ILateUpdatable> _pendingLateUpdatables = new List<ILateUpdatable>(16);

        private bool _isUpdating;
        private bool _isFixedUpdating;
        private bool _isLateUpdating;

        private bool _needsUpdatableCompaction;
        private bool _needsFixedUpdatableCompaction;
        private bool _needsLateUpdatableCompaction;

        protected override void Awake()
        {
            base.Awake();
        }

        private void Update()
        {
            CommitPendingUpdatables();

            _isUpdating = true;
            float dt = Time.deltaTime;

            for (int i = 0; i < _updatables.Count; i++)
            {
                var item = _updatables[i];
                if (item == null)
                    continue;

                if (item is UnityEngine.Object unityObj && unityObj == null)
                {
                    NLogger.LogError($"Target '{item.GetType().Name}' was destroyed without unregistering from GameLoop. Auto-unregistering.");
                    UnregisterInternal(item);
                    continue;
                }

                try
                {
                    item.OnUpdate(dt);
                }
                catch (Exception ex)
                {
                    NLogger.LogException(ex);
                }
            }

            _isUpdating = false;

            if (_needsUpdatableCompaction)
            {
                CompactList(_updatables);
                _needsUpdatableCompaction = false;
            }
        }

        private void FixedUpdate()
        {
            CommitPendingFixedUpdatables();

            _isFixedUpdating = true;
            float fixedDt = Time.fixedDeltaTime;

            for (int i = 0; i < _fixedUpdatables.Count; i++)
            {
                var item = _fixedUpdatables[i];
                if (item == null)
                    continue;

                if (item is UnityEngine.Object unityObj && unityObj == null)
                {
                    NLogger.LogError($"Target '{item.GetType().Name}' was destroyed without unregistering from GameLoop. Auto-unregistering.");
                    UnregisterInternal(item);
                    continue;
                }

                try
                {
                    item.OnFixedUpdate(fixedDt);
                }
                catch (Exception ex)
                {
                    NLogger.LogException(ex);
                }
            }

            _isFixedUpdating = false;

            if (_needsFixedUpdatableCompaction)
            {
                CompactList(_fixedUpdatables);
                _needsFixedUpdatableCompaction = false;
            }
        }

        private void LateUpdate()
        {
            CommitPendingLateUpdatables();

            _isLateUpdating = true;
            float dt = Time.deltaTime;

            for (int i = 0; i < _lateUpdatables.Count; i++)
            {
                var item = _lateUpdatables[i];
                if (item == null)
                    continue;

                if (item is UnityEngine.Object unityObj && unityObj == null)
                {
                    NLogger.LogError($"Target '{item.GetType().Name}' was destroyed without unregistering from GameLoop. Auto-unregistering.");
                    UnregisterInternal(item);
                    continue;
                }

                try
                {
                    item.OnLateUpdate(dt);
                }
                catch (Exception ex)
                {
                    NLogger.LogException(ex);
                }
            }

            _isLateUpdating = false;

            if (_needsLateUpdatableCompaction)
            {
                CompactList(_lateUpdatables);
                _needsLateUpdatableCompaction = false;
            }
        }

        public static void Register(object target)
        {
            if (target == null)
            {
                NLogger.LogError("Cannot register null target to GameLoop.");
                return;
            }

            I.RegisterInternal(target);
        }

        public static void Unregister(object target)
        {
            if (!HasInstance)
                return;

            if (target == null)
            {
                NLogger.LogError("Cannot unregister null target from GameLoop.");
                return;
            }

            I.UnregisterInternal(target);
        }

        private void RegisterInternal(object target)
        {
            if (!_registeredObjects.Add(target))
            {
                NLogger.LogError($"Target '{target.GetType().Name}' is already registered in GameLoop.");
                return;
            }

            bool implementsAny = false;

            if (target is IUpdatable updatable)
            {
                implementsAny = true;
                if (_isUpdating)
                    _pendingUpdatables.Add(updatable);
                else
                    _updatables.Add(updatable);
            }

            if (target is IFixedUpdatable fixedUpdatable)
            {
                implementsAny = true;
                if (_isFixedUpdating)
                    _pendingFixedUpdatables.Add(fixedUpdatable);
                else
                    _fixedUpdatables.Add(fixedUpdatable);
            }

            if (target is ILateUpdatable lateUpdatable)
            {
                implementsAny = true;
                if (_isLateUpdating)
                    _pendingLateUpdatables.Add(lateUpdatable);
                else
                    _lateUpdatables.Add(lateUpdatable);
            }

            if (!implementsAny)
            {
                _registeredObjects.Remove(target);
                NLogger.LogError($"Target '{target.GetType().Name}' does not implement IUpdatable, IFixedUpdatable, or ILateUpdatable.");
            }
        }

        private void UnregisterInternal(object target)
        {
            if (!_registeredObjects.Remove(target))
            {
                NLogger.LogError($"Target '{target.GetType().Name}' is not registered in GameLoop.");
                return;
            }

            if (target is IUpdatable updatable)
            {
                int pendingIndex = _pendingUpdatables.IndexOf(updatable);
                if (pendingIndex >= 0)
                {
                    _pendingUpdatables.RemoveAt(pendingIndex);
                }

                int index = _updatables.IndexOf(updatable);
                if (index >= 0)
                {
                    _updatables[index] = null;
                    _needsUpdatableCompaction = true;
                }
            }

            if (target is IFixedUpdatable fixedUpdatable)
            {
                int pendingIndex = _pendingFixedUpdatables.IndexOf(fixedUpdatable);
                if (pendingIndex >= 0)
                {
                    _pendingFixedUpdatables.RemoveAt(pendingIndex);
                }

                int index = _fixedUpdatables.IndexOf(fixedUpdatable);
                if (index >= 0)
                {
                    _fixedUpdatables[index] = null;
                    _needsFixedUpdatableCompaction = true;
                }
            }

            if (target is ILateUpdatable lateUpdatable)
            {
                int pendingIndex = _pendingLateUpdatables.IndexOf(lateUpdatable);
                if (pendingIndex >= 0)
                {
                    _pendingLateUpdatables.RemoveAt(pendingIndex);
                }

                int index = _lateUpdatables.IndexOf(lateUpdatable);
                if (index >= 0)
                {
                    _lateUpdatables[index] = null;
                    _needsLateUpdatableCompaction = true;
                }
            }
        }

        private void CommitPendingUpdatables()
        {
            if (_pendingUpdatables.Count == 0)
                return;

            for (int i = 0; i < _pendingUpdatables.Count; i++)
            {
                _updatables.Add(_pendingUpdatables[i]);
            }
            _pendingUpdatables.Clear();
        }

        private void CommitPendingFixedUpdatables()
        {
            if (_pendingFixedUpdatables.Count == 0)
                return;

            for (int i = 0; i < _pendingFixedUpdatables.Count; i++)
            {
                _fixedUpdatables.Add(_pendingFixedUpdatables[i]);
            }
            _pendingFixedUpdatables.Clear();
        }

        private void CommitPendingLateUpdatables()
        {
            if (_pendingLateUpdatables.Count == 0)
                return;

            for (int i = 0; i < _pendingLateUpdatables.Count; i++)
            {
                _lateUpdatables.Add(_pendingLateUpdatables[i]);
            }
            _pendingLateUpdatables.Clear();
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
