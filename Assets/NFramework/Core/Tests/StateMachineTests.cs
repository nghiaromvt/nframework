using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace NFramework.Core.Tests
{
    [TestFixture]
    public class StateMachineTests
    {
        private class StateA : StateBase
        {
            public List<string> EventLog { get; set; }
            public float TotalUpdateDelta { get; private set; }

            public override void OnEnter(EnterStateData data = null)
            {
                EventLog?.Add("StateA.OnEnter");
            }

            public override void OnExit()
            {
                EventLog?.Add("StateA.OnExit");
            }

            public override void OnUpdate(float deltaTime)
            {
                TotalUpdateDelta += deltaTime;
                EventLog?.Add("StateA.OnUpdate");
            }
        }

        private class StateB : StateBase
        {
            public List<string> EventLog { get; set; }

            public override void OnEnter(EnterStateData data = null)
            {
                EventLog?.Add("StateB.OnEnter");
            }

            public override void OnExit()
            {
                EventLog?.Add("StateB.OnExit");
            }
        }

        private class UnregisteredState : StateBase { }

        [Test]
        public void RegisterState_NullState_ThrowsArgumentNullException()
        {
            var sm = new StateMachine();
            Assert.Throws<ArgumentNullException>(() => sm.RegisterState<StateA>(null));
        }

        [Test]
        public void RegisterState_DuplicateState_ThrowsInvalidOperationException()
        {
            var sm = new StateMachine();
            sm.RegisterState(new StateA());

            Assert.Throws<InvalidOperationException>(() => sm.RegisterState(new StateA()));
        }

        [Test]
        public void ChangeState_UnregisteredState_ThrowsInvalidOperationException()
        {
            var sm = new StateMachine();
            sm.RegisterState(new StateA());

            Assert.Throws<InvalidOperationException>(() => sm.ChangeState<UnregisteredState>());
        }

        [Test]
        public void ChangeState_ExecutesOnExitThenOnEnterInOrder()
        {
            var events = new List<string>();
            var sm = new StateMachine();
            var stateA = new StateA { EventLog = events };
            var stateB = new StateB { EventLog = events };

            sm.RegisterState(stateA);
            sm.RegisterState(stateB);

            sm.ChangeState<StateA>();
            Assert.AreSame(stateA, sm.CurrentState);
            Assert.IsNull(sm.PreviousState);
            CollectionAssert.AreEqual(new[] { "StateA.OnEnter" }, events);

            events.Clear();
            sm.ChangeState<StateB>();

            Assert.AreSame(stateB, sm.CurrentState);
            Assert.AreSame(stateA, sm.PreviousState);
            CollectionAssert.AreEqual(new[] { "StateA.OnExit", "StateB.OnEnter" }, events);
        }

        [Test]
        public void OnUpdate_TicksCurrentState()
        {
            var events = new List<string>();
            var sm = new StateMachine();
            var stateA = new StateA { EventLog = events };

            sm.RegisterState(stateA);
            sm.ChangeState<StateA>();

            events.Clear();
            sm.OnUpdate(0.16f);

            Assert.AreEqual(0.16f, stateA.TotalUpdateDelta, 0.0001f);
            CollectionAssert.AreEqual(new[] { "StateA.OnUpdate" }, events);
        }

        [Test]
        public void Stop_CallsOnExit_ClearsCurrentState()
        {
            var events = new List<string>();
            var sm = new StateMachine();
            var stateA = new StateA { EventLog = events };

            sm.RegisterState(stateA);
            sm.ChangeState<StateA>();

            events.Clear();
            sm.Stop();

            Assert.IsNull(sm.CurrentState);
            Assert.IsFalse(sm.IsRunning);
        }

        [Test]
        public void GetState_ReturnsRegisteredInstance()
        {
            var sm = new StateMachine();
            var stateA = new StateA();
            sm.RegisterState(stateA);

            Assert.AreSame(stateA, sm.GetState<StateA>());
            Assert.IsTrue(sm.TryGetState<StateA>(out var found));
            Assert.AreSame(stateA, found);
        }
    }
}
