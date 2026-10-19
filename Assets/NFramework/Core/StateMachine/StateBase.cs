using System;

namespace NFramework
{
    [Serializable]
    public abstract class StateBase
    {
        public StateMachine StateMachine { get; internal set; }

        public virtual void OnEnter(EnterStateData data = null) { }

        public virtual void OnExit() { }

        public virtual void OnUpdate() { }

        public virtual void OnUpdate(float deltaTime) => OnUpdate();

        public virtual void OnFixedUpdate() { }

        public virtual void OnFixedUpdate(float fixedDeltaTime) => OnFixedUpdate();

        public virtual void OnLateUpdate() { }

        public virtual void OnLateUpdate(float deltaTime) => OnLateUpdate();
    }
}