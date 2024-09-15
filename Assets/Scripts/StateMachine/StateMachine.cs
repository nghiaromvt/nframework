using RotaryHeart.Lib.SerializableDictionary;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NFramework
{
    public class StateMachine : MonoBehaviour
    {
        public bool isRunning = true;

        [ReadOnly, SerializeField] private SerializableDictionaryBase<string, State> _stateDic = new();
        [ReadOnly, SerializeField, SerializeReference] private State _currentState;
        [ReadOnly, SerializeField, SerializeReference] private State _previousState;

        public bool IsInitialized { get; private set; }
        public State CurrentState => _currentState;
        public State PreviousState => _previousState;

        private void Update()
        {
            if (isRunning && _currentState != null)
                _currentState.OnUpdate();
        }

        private void FixedUpdate()
        {
            if (isRunning && _currentState != null)
                _currentState.OnFixedUpdate();
        }

        private void LateUpdate()
        {
            if (isRunning && _currentState != null)
                _currentState.OnLateUpdate();
        }

        public void Init(List<State> states, string defaultStateId = "", bool forceInitStates = false)
        {
            if (IsInitialized)
                return;

            IsInitialized = true;

            foreach (var state in states)
            {
                _stateDic.Add(state.Id, state);
                if (forceInitStates)
                    state.Init(this);
            }

            if (!defaultStateId.IsNullOrEmpty())
                SetState(defaultStateId);
        }

        public void SetState(string stateId)
        {
            if (_stateDic.TryGetValue(stateId, out var state))
            {
                _currentState?.OnExit();
                _previousState = _currentState;
                _currentState = state;
                _currentState.Init(this);
                _currentState.OnEnter();
            }
            else
            {
                Debug.Log($"Not found state with id:{stateId}", gameObject);
            }
        }

        public State GetState(string stateId) => _stateDic.ContainsKey(stateId) ? _stateDic[stateId] : null;

        public List<State> GetAllStates() => _stateDic.Values.ToList();
    }

    [System.Serializable]
    public class State
    {
        [ReadOnly, SerializeField] private string _id;

        public string Id => _id;
        public StateMachine StateMachine { get; private set; }
        public bool IsInitialized { get; private set; }

        public State(string id) => _id = id;

        public virtual bool Init(StateMachine stateMachine)
        {
            if (IsInitialized)
                return false;

            IsInitialized = true;
            StateMachine = stateMachine;
            return true;
        }

        public virtual void OnEnter() { }

        public virtual void OnExit() { }

        public virtual void OnUpdate() { }

        public virtual void OnFixedUpdate() { }

        public virtual void OnLateUpdate() { }
    }
}
