using System;
using System.Collections.Generic;

namespace NFramework
{
    public class StateMachine : IUpdatable, IFixedUpdatable, ILateUpdatable
    {
        private readonly Dictionary<Type, StateBase> _states = new();
        private bool _isStarted;

        public StateBase CurrentState { get; private set; }
        public StateBase PreviousState { get; private set; }
        public bool IsRunning => _isStarted;

        public void RegisterState<TState>(TState state) where TState : StateBase
        {
            RegisterState((StateBase)state);
        }

        public void RegisterState(StateBase state)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            Type type = state.GetType();
            if (_states.ContainsKey(type))
            {
                throw new InvalidOperationException($"State of type {type.Name} is already registered in {nameof(StateMachine)}.");
            }

            state.StateMachine = this;
            _states.Add(type, state);
        }

        public void RegisterStates(params StateBase[] states)
        {
            if (states == null)
            {
                throw new ArgumentNullException(nameof(states));
            }

            for (int i = 0; i < states.Length; i++)
            {
                RegisterState(states[i]);
            }
        }

        public void Start<TDefaultState>(EnterStateData data = null) where TDefaultState : StateBase
        {
            Start(typeof(TDefaultState), data);
        }

        public void Start(Type defaultStateType = null, EnterStateData data = null)
        {
            if (_isStarted)
            {
                throw new InvalidOperationException($"{nameof(StateMachine)} is already running.");
            }

            _isStarted = true;
            GameLoop.Register(this);

            if (defaultStateType != null)
            {
                ChangeState(defaultStateType, data);
            }
        }

        public void Stop()
        {
            if (!_isStarted && CurrentState == null)
            {
                return;
            }

            if (GameLoop.HasInstance)
            {
                GameLoop.Unregister(this);
            }

            CurrentState?.OnExit();
            PreviousState = CurrentState;
            CurrentState = null;
            _isStarted = false;
        }

        public void ChangeState<TState>(EnterStateData data = null) where TState : StateBase
        {
            ChangeState(typeof(TState), data);
        }

        public void ChangeState(Type stateType, EnterStateData data = null)
        {
            if (stateType == null)
            {
                throw new ArgumentNullException(nameof(stateType));
            }

            if (!_states.TryGetValue(stateType, out var targetState))
            {
                throw new InvalidOperationException($"State of type {stateType.Name} is not registered in {nameof(StateMachine)}.");
            }

            CurrentState?.OnExit();
            PreviousState = CurrentState;
            CurrentState = targetState;
            CurrentState.OnEnter(data);
        }

        public TState GetState<TState>() where TState : StateBase
        {
            Type type = typeof(TState);
            if (_states.TryGetValue(type, out var state))
            {
                return (TState)state;
            }

            throw new InvalidOperationException($"State of type {type.Name} is not registered in {nameof(StateMachine)}.");
        }

        public bool TryGetState<TState>(out TState state) where TState : StateBase
        {
            Type type = typeof(TState);
            if (_states.TryGetValue(type, out var foundState))
            {
                state = (TState)foundState;
                return true;
            }

            state = null;
            return false;
        }

        public void OnUpdate(float deltaTime)
        {
            CurrentState?.OnUpdate(deltaTime);
        }

        public void OnFixedUpdate(float fixedDeltaTime)
        {
            CurrentState?.OnFixedUpdate(fixedDeltaTime);
        }

        public void OnLateUpdate(float deltaTime)
        {
            CurrentState?.OnLateUpdate(deltaTime);
        }
    }

    public class EnterStateData { }
}