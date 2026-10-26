using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace NFramework.Core.Tests
{
    [TestFixture]
    public class ObserverTests
    {
        private struct TestEvent : IEvent
        {
            public int Value;

            public TestEvent(int value)
            {
                Value = value;
            }
        }

        [SetUp]
        public void Setup()
        {
            EventBus<TestEvent>.Clear();
        }

        [TearDown]
        public void Teardown()
        {
            EventBus<TestEvent>.Clear();
        }

        #region EventBus Tests

        [Test]
        public void EventBus_SubscribeAndRaise_HandlerInvoked()
        {
            var receivedValue = 0;
            EventBus<TestEvent>.Subscribe(evt => receivedValue = evt.Value);

            EventBus<TestEvent>.Raise(new TestEvent(42));

            Assert.AreEqual(42, receivedValue);
        }

        [Test]
        public void EventBus_Unsubscribe_HandlerNotInvoked()
        {
            var callCount = 0;
            Action<TestEvent> handler = _ => callCount++;

            EventBus<TestEvent>.Subscribe(handler);
            EventBus<TestEvent>.Raise(new TestEvent(1));
            Assert.AreEqual(1, callCount);

            EventBus<TestEvent>.Unsubscribe(handler);
            EventBus<TestEvent>.Raise(new TestEvent(2));
            Assert.AreEqual(1, callCount);
        }

        [Test]
        public void EventBus_DisposableSubscription_DisposesCorrectly()
        {
            var callCount = 0;
            var sub = EventBus<TestEvent>.Subscribe(_ => callCount++);

            EventBus<TestEvent>.Raise(new TestEvent(1));
            Assert.AreEqual(1, callCount);

            sub.Dispose();
            EventBus<TestEvent>.Raise(new TestEvent(2));
            Assert.AreEqual(1, callCount);
        }

        [Test]
        public void EventBus_SubscribeNull_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => EventBus<TestEvent>.Subscribe(null));
        }

        [Test]
        public void EventBus_Clear_RemovesAllSubscribers()
        {
            var callCount = 0;
            EventBus<TestEvent>.Subscribe(_ => callCount++);
            EventBus<TestEvent>.Clear();

            EventBus<TestEvent>.Raise(new TestEvent(1));
            Assert.AreEqual(0, callCount);
        }

        #endregion

        #region ObservableValue Tests

        [Test]
        public void ObservableValue_ChangeValue_FiresEvents()
        {
            var obs = new ObservableValue<int>(10);
            var newReported = 0;
            var oldReported = 0;

            obs.OnValueChanged += val => newReported = val;
            obs.OnValueChangedWithPrevious += (oldVal, newVal) =>
            {
                oldReported = oldVal;
            };

            obs.Value = 25;

            Assert.AreEqual(25, obs.Value);
            Assert.AreEqual(25, newReported);
            Assert.AreEqual(10, oldReported);
        }

        [Test]
        public void ObservableValue_SetSameValue_DoesNotFireEvent()
        {
            var obs = new ObservableValue<int>(10);
            var callCount = 0;
            obs.OnValueChanged += _ => callCount++;

            obs.Value = 10;

            Assert.AreEqual(0, callCount);
        }

        [Test]
        public void ObservableValue_SetValueWithoutNotify_DoesNotFireEvent()
        {
            var obs = new ObservableValue<int>(10);
            var callCount = 0;
            obs.OnValueChanged += _ => callCount++;

            obs.SetValueWithoutNotify(30);

            Assert.AreEqual(30, obs.Value);
            Assert.AreEqual(0, callCount);
        }

        [Test]
        public void ObservableValue_SetAndNotify_FiresEvenIfSameValue()
        {
            var obs = new ObservableValue<int>(10);
            var callCount = 0;
            obs.OnValueChanged += _ => callCount++;

            obs.SetAndNotify(10);

            Assert.AreEqual(1, callCount);
        }

        [Test]
        public void ObservableValue_ForceNotify_FiresCurrentValue()
        {
            var obs = new ObservableValue<int>(50);
            var received = 0;
            obs.OnValueChanged += val => received = val;

            obs.ForceNotify();

            Assert.AreEqual(50, received);
        }

        [Test]
        public void ObservableValue_SubscribeImmediate_FiresImmediately()
        {
            var obs = new ObservableValue<int>(99);
            var received = 0;

            using (obs.Subscribe(val => received = val, invokeImmediately: true))
            {
                Assert.AreEqual(99, received);
                obs.Value = 100;
                Assert.AreEqual(100, received);
            }

            obs.Value = 101;
            Assert.AreEqual(100, received);
        }

        #endregion

        #region ObservableList Tests

        [Test]
        public void ObservableList_Add_FiresAddEvent()
        {
            var list = new ObservableList<string>();
            ObservableListEvent<string> lastEvent = default;
            list.OnChanged += evt => lastEvent = evt;

            list.Add("Alpha");

            Assert.AreEqual(1, list.Count);
            Assert.AreEqual(ObservableListEventType.Add, lastEvent.Type);
            Assert.AreEqual(0, lastEvent.Index);
            Assert.AreEqual("Alpha", lastEvent.Item);
        }

        [Test]
        public void ObservableList_Insert_FiresAddEventWithIndex()
        {
            var list = new ObservableList<string> { "A", "C" };
            ObservableListEvent<string> lastEvent = default;
            list.OnChanged += evt => lastEvent = evt;

            list.Insert(1, "B");

            Assert.AreEqual(3, list.Count);
            Assert.AreEqual(ObservableListEventType.Add, lastEvent.Type);
            Assert.AreEqual(1, lastEvent.Index);
            Assert.AreEqual("B", lastEvent.Item);
            Assert.AreEqual("B", list[1]);
        }

        [Test]
        public void ObservableList_Remove_FiresRemoveEvent()
        {
            var list = new ObservableList<string> { "X", "Y", "Z" };
            ObservableListEvent<string> lastEvent = default;
            list.OnChanged += evt => lastEvent = evt;

            var removed = list.Remove("Y");

            Assert.IsTrue(removed);
            Assert.AreEqual(2, list.Count);
            Assert.AreEqual(ObservableListEventType.Remove, lastEvent.Type);
            Assert.AreEqual(1, lastEvent.Index);
            Assert.AreEqual("Y", lastEvent.Item);
        }

        [Test]
        public void ObservableList_IndexerReplace_FiresReplaceEvent()
        {
            var list = new ObservableList<string> { "Old" };
            ObservableListEvent<string> lastEvent = default;
            list.OnChanged += evt => lastEvent = evt;

            list[0] = "New";

            Assert.AreEqual(ObservableListEventType.Replace, lastEvent.Type);
            Assert.AreEqual(0, lastEvent.Index);
            Assert.AreEqual("New", lastEvent.Item);
            Assert.AreEqual("Old", lastEvent.OldItem);
        }

        [Test]
        public void ObservableList_Clear_FiresClearEvent()
        {
            var list = new ObservableList<int> { 1, 2, 3 };
            ObservableListEvent<int> lastEvent = default;
            list.OnChanged += evt => lastEvent = evt;

            list.Clear();

            Assert.AreEqual(0, list.Count);
            Assert.AreEqual(ObservableListEventType.Clear, lastEvent.Type);
        }

        [Test]
        public void ObservableList_Reset_FiresResetEvent()
        {
            var list = new ObservableList<int> { 1, 2 };
            ObservableListEvent<int> lastEvent = default;
            list.OnChanged += evt => lastEvent = evt;

            list.Reset(new[] { 10, 20, 30 });

            Assert.AreEqual(3, list.Count);
            Assert.AreEqual(ObservableListEventType.Reset, lastEvent.Type);
            Assert.AreEqual(10, list[0]);
        }

        [Test]
        public void ObservableList_Foreach_IteratesAllItems()
        {
            var list = new ObservableList<int> { 1, 2, 3 };
            var sum = 0;

            foreach (var item in list)
            {
                sum += item;
            }

            Assert.AreEqual(6, sum);
        }

        #endregion
    }
}
