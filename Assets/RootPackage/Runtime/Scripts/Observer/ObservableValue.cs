using System;
using System.Collections.Generic;

namespace NFramework
{
    public class ObservableValue<T>
    {
        public event Action<T> OnValueChanged;

        private T _value;

        public T Value
        {
            get => _value;
            set
            {
                if (EqualityComparer<T>.Default.Equals(_value, value))
                    return;

                _value = value;
                OnValueChanged?.Invoke(_value);
            }
        }

        public ObservableValue() { }

        public ObservableValue(T initValue) => _value = initValue;

        public void SetAndNotify(T value)
        {
            _value = value;
            OnValueChanged?.Invoke(_value);
        }

        public void SetSilently(T value) => _value = value;
        
        public override string ToString() => _value?.ToString() ?? "null";
        
        public static implicit operator T(ObservableValue<T> observable) => observable.Value;
    }
}