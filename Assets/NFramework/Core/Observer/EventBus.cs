using System;

namespace NFramework
{
    public static class EventBus<T> where T : IEvent
    {
        private static Action<T> _onEvent;

        static EventBus()
        {
            EventBusResetRegistry.Register(Clear);
        }

        public static EventSubscription<T> Subscribe(Action<T> handler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            _onEvent += handler;
            return new EventSubscription<T>(handler);
        }

        public static void Unsubscribe(Action<T> handler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            _onEvent -= handler;
        }

        public static void Raise(in T eventData)
        {
            _onEvent?.Invoke(eventData);
        }

        public static void Clear()
        {
            _onEvent = null;
        }
    }

    public readonly struct EventSubscription<T> : IDisposable where T : IEvent
    {
        private readonly Action<T> _handler;

        public EventSubscription(Action<T> handler)
        {
            _handler = handler;
        }

        public void Dispose()
        {
            if (_handler != null)
            {
                EventBus<T>.Unsubscribe(_handler);
            }
        }
    }
}
