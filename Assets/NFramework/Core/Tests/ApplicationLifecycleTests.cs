using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace NFramework.Core.Tests
{
    [TestFixture]
    public class ApplicationLifecycleTests
    {
        private class TestListener : IApplicationPauseListener, IApplicationFocusListener, IApplicationQuitListener
        {
            public int PauseCount { get; private set; }
            public bool LastPauseStatus { get; private set; }

            public int FocusCount { get; private set; }
            public bool LastFocusStatus { get; private set; }

            public int QuitCount { get; private set; }

            public void OnApplicationPause(bool isPaused)
            {
                PauseCount++;
                LastPauseStatus = isPaused;
            }

            public void OnApplicationFocus(bool hasFocus)
            {
                FocusCount++;
                LastFocusStatus = hasFocus;
            }

            public void OnApplicationQuit()
            {
                QuitCount++;
            }
        }

        private class ReentrantPauseListener : IApplicationPauseListener
        {
            private readonly Action _onPauseAction;

            public ReentrantPauseListener(Action onPauseAction)
            {
                _onPauseAction = onPauseAction;
            }

            public void OnApplicationPause(bool isPaused)
            {
                _onPauseAction?.Invoke();
            }
        }

        private class FaultyListener : IApplicationPauseListener
        {
            public void OnApplicationPause(bool isPaused)
            {
                throw new InvalidOperationException("Simulated exception in listener");
            }
        }

        [TearDown]
        public void TearDown()
        {
            if (ApplicationLifecycle.HasInstance)
            {
                UnityEngine.Object.DestroyImmediate(ApplicationLifecycle.I.gameObject);
            }
        }

        private static void TriggerPause(bool pauseStatus)
        {
            var method = typeof(ApplicationLifecycle).GetMethod("OnApplicationPause", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(method, "OnApplicationPause method should exist.");
            method.Invoke(ApplicationLifecycle.I, new object[] { pauseStatus });
        }

        private static void TriggerFocus(bool hasFocus)
        {
            var method = typeof(ApplicationLifecycle).GetMethod("OnApplicationFocus", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(method, "OnApplicationFocus method should exist.");
            method.Invoke(ApplicationLifecycle.I, new object[] { hasFocus });
        }

        private static void TriggerQuit()
        {
            var method = typeof(ApplicationLifecycle).GetMethod("OnApplicationQuit", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(method, "OnApplicationQuit method should exist.");
            method.Invoke(ApplicationLifecycle.I, null);
        }

        [Test]
        public void RegisterAndDispatch_PauseFocusQuit_Success()
        {
            var listener = new TestListener();
            ApplicationLifecycle.Register(listener);

            TriggerPause(true);
            Assert.AreEqual(1, listener.PauseCount);
            Assert.IsTrue(listener.LastPauseStatus);
            Assert.IsTrue(ApplicationLifecycle.IsPaused);

            TriggerFocus(false);
            Assert.AreEqual(1, listener.FocusCount);
            Assert.IsFalse(listener.LastFocusStatus);
            Assert.IsFalse(ApplicationLifecycle.IsFocused);

            TriggerQuit();
            Assert.AreEqual(1, listener.QuitCount);
            Assert.IsTrue(ApplicationLifecycle.IsQuitting);

            ApplicationLifecycle.Unregister(listener);
        }

        [Test]
        public void Unregister_StopsReceivingCallbacks()
        {
            var listener = new TestListener();
            ApplicationLifecycle.Register(listener);

            TriggerPause(true);
            Assert.AreEqual(1, listener.PauseCount);

            ApplicationLifecycle.Unregister(listener);

            TriggerPause(false);
            Assert.AreEqual(1, listener.PauseCount);
        }

        [Test]
        public void Reentrancy_UnregisterDuringCallback_DoesNotThrow()
        {
            ReentrantPauseListener listener = null;
            listener = new ReentrantPauseListener(() =>
            {
                ApplicationLifecycle.Unregister(listener);
            });

            var normalListener = new TestListener();

            ApplicationLifecycle.Register(listener);
            ApplicationLifecycle.Register(normalListener);

            Assert.DoesNotThrow(() => TriggerPause(true));
            Assert.AreEqual(1, normalListener.PauseCount);

            // After unregister, triggering again should not invoke listener
            Assert.DoesNotThrow(() => TriggerPause(false));
            Assert.AreEqual(2, normalListener.PauseCount);

            ApplicationLifecycle.Unregister(normalListener);
        }

        [Test]
        public void ExceptionIsolation_OneListenerThrows_OtherListenersStillReceiveCallback()
        {
            var faultyListener = new FaultyListener();
            var normalListener = new TestListener();

            ApplicationLifecycle.Register(faultyListener);
            ApplicationLifecycle.Register(normalListener);

            Assert.DoesNotThrow(() => TriggerPause(true));
            Assert.AreEqual(1, normalListener.PauseCount);

            ApplicationLifecycle.Unregister(faultyListener);
            ApplicationLifecycle.Unregister(normalListener);
        }
    }
}
