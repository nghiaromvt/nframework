using System;
using Sirenix.OdinInspector;

namespace NFramework
{
    [Serializable]
    public class StateBase
    {
        [NonSerialized] protected StateMachine _stateMachine;
        
        [ShowInInspector, ReadOnly] public string Id { get; private set; }

        public StateBase(string id, StateMachine stateMachine)
        {
            Id = id;
            _stateMachine = stateMachine;
        }

        public virtual void OnEnter(EnterStateData data = null) { }

        public virtual void OnExit() { }

        public virtual void OnUpdate() { }

        public virtual void OnFixedUpdate() { }

        public virtual void OnLateUpdate() { }
    }
}