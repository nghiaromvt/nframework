using System;
using System.Collections.Generic;
using UnityEngine;

namespace NFramework
{
    [Serializable]
    public class ObservableValue<T>
    {
        [SerializeField] private T _value;

        public event Action<T> OnValueChanged;
        public event Action<T, T> OnValueChangedWithPrevious;

        public T Value
        {
            get => _value;
            set
            {
                if (EqualityComparer<T>.Default.Equals(_value, value))
                {
                    return;
                }

                var previous = _value;
                _value = value;
                OnValueChanged?.Invoke(_value);
                OnValueChangedWithPrevious?.Invoke(previous, _value);
            }
        }

        public ObservableValue()
        {
        }

        public ObservableValue(T initValue)
        {
            _value = initValue;
        }

        public void SetValueWithoutNotify(T value)
        {
            _value = value;
        }

        public void SetAndNotify(T value)
        {
            var previous = _value;
            _value = value;
            OnValueChanged?.Invoke(_value);
            OnValueChangedWithPrevious?.Invoke(previous, _value);
        }

        public void ForceNotify()
        {
            OnValueChanged?.Invoke(_value);
            OnValueChangedWithPrevious?.Invoke(_value, _value);
        }

        public IDisposable Subscribe(Action<T> callback, bool invokeImmediately = false)
        {
            if (callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }

            OnValueChanged += callback;

            if (invokeImmediately)
            {
                callback.Invoke(_value);
            }

            return new Subscription(this, callback);
        }

        public override string ToString()
        {
            return _value?.ToString() ?? "null";
        }

        public static implicit operator T(ObservableValue<T> observable)
        {
            return observable.Value;
        }

        private sealed class Subscription : IDisposable
        {
            private readonly ObservableValue<T> _observable;
            private readonly Action<T> _callback;

            public Subscription(ObservableValue<T> observable, Action<T> callback)
            {
                _observable = observable;
                _callback = callback;
            }

            public void Dispose()
            {
                _observable.OnValueChanged -= _callback;
            }
        }
    }
}
