using System;
using UnityEngine;

namespace NFramework
{
    public interface IPool : IDisposable
    {
        Component Prefab { get; }
        int ActiveCount { get; }
        int IdleCount { get; }
        int MaxSize { get; }
        PoolScope Scope { get; }

        void Despawn(Component instance);
        void DespawnAll();
        void Clear();
        void OnInstanceDestroyed(Component instance);
    }
}
