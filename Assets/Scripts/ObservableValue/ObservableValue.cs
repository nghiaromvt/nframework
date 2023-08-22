using System;

namespace NFramework
{
    [Serializable]
    public class ObservableValue<T>
    {
        public event Action<T> OnValueChanged;

        private T _value = default;

        public T Value
        {
            get => _value;
            set
            {
                if (_value != null && _value.Equals(value))
                    return;

                _value = value;
                OnValueChanged?.Invoke(_value);
            }
        }

        public ObservableValue() { }

        public ObservableValue(T initValue) => _value = initValue;

        public void ForceSet(T value)
        {
            _value = value;
            OnValueChanged?.Invoke(_value);
        }

        public void SetWithoutNotify(T value) => _value = value;
    }
}
