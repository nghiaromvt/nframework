using System;
using System.Collections.Generic;
using UnityEngine;

namespace NFramework
{
    internal static class SingletonResetRegistry
    {
        private static readonly List<Action> _resetters = new();

        public static void Register(Action resetAction)
        {
            if (resetAction != null && !_resetters.Contains(resetAction))
            {
                _resetters.Add(resetAction);
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetAll()
        {
            for (int i = 0; i < _resetters.Count; i++)
            {
                _resetters[i]?.Invoke();
            }
        }
    }
}
