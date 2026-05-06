using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace NFramework
{
    [System.Serializable]
    public class StateMachine
    {
        protected UnitySerializedDictionary<string, StateBase> _stateDic = new();

        [ReadOnly, ShowInInspector] public StateBase CurrentState { get; private set; }
        [ReadOnly, ShowInInspector] public StateBase PreviousState { get; private set; }
        
        public void Init(string defaultStateId, params StateBase[] states)
        {
            foreach (var state in states)
            {
                _stateDic.Add(state.Id, state);
            }

            if (!defaultStateId.IsNullOrEmpty())
                ChangeState(defaultStateId);
        }
        
        public void ChangeState(string stateId, EnterStateData data = null)
        {
            if (_stateDic.TryGetValue(stateId, out var state))
            {
                CurrentState?.OnExit();
                PreviousState = CurrentState;
                CurrentState = state;
                CurrentState.OnEnter(data);
            }
            else
            {
                Debug.LogError($"Not found state: {stateId}");
            }
        }

        public StateBase GetState(string stateId) => _stateDic.ContainsKey(stateId) ? _stateDic[stateId] : null;

        public List<StateBase> GetAllStates() => _stateDic.Values.ToList();
    }
    
    public class EnterStateData { }
}