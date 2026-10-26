using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace NFramework
{
    public enum ObservableListEventType
    {
        Add,
        Remove,
        Replace,
        Clear,
        Reset
    }

    public readonly struct ObservableListEvent<TItem>
    {
        public ObservableListEventType Type { get; }
        public int Index { get; }
        public TItem Item { get; }
        public TItem OldItem { get; }

        public ObservableListEvent(ObservableListEventType type, int index, TItem item, TItem oldItem = default)
        {
            Type = type;
            Index = index;
            Item = item;
            OldItem = oldItem;
        }
    }

    [Serializable]
    public class ObservableList<T> : IList<T>, IReadOnlyList<T>
    {
        [SerializeField] private List<T> _list;

        private ReadOnlyCollection<T> _readOnlyWrapper;

        public event Action<ObservableListEvent<T>> OnChanged;

        public int Count => _list.Count;

        public bool IsReadOnly => false;

        public ReadOnlyCollection<T> ReadOnlyList => _readOnlyWrapper ??= _list.AsReadOnly();

        public T this[int index]
        {
            get => _list[index];
            set
            {
                if (EqualityComparer<T>.Default.Equals(_list[index], value))
                {
                    return;
                }

                var oldItem = _list[index];
                _list[index] = value;
                Notify(new ObservableListEvent<T>(ObservableListEventType.Replace, index, value, oldItem));
            }
        }

        public ObservableList()
        {
            _list = new List<T>();
        }

        public ObservableList(int capacity)
        {
            _list = new List<T>(capacity);
        }

        public ObservableList(IEnumerable<T> collection)
        {
            _list = collection != null ? new List<T>(collection) : new List<T>();
        }

        public void Add(T item)
        {
            _list.Add(item);
            Notify(new ObservableListEvent<T>(ObservableListEventType.Add, _list.Count - 1, item));
        }

        public void AddRange(IEnumerable<T> collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            foreach (var item in collection)
            {
                Add(item);
            }
        }

        public void Insert(int index, T item)
        {
            _list.Insert(index, item);
            Notify(new ObservableListEvent<T>(ObservableListEventType.Add, index, item));
        }

        public bool Remove(T item)
        {
            var index = _list.IndexOf(item);
            if (index < 0)
            {
                return false;
            }

            _list.RemoveAt(index);
            Notify(new ObservableListEvent<T>(ObservableListEventType.Remove, index, item));
            return true;
        }

        public void RemoveAt(int index)
        {
            var item = _list[index];
            _list.RemoveAt(index);
            Notify(new ObservableListEvent<T>(ObservableListEventType.Remove, index, item));
        }

        public void Clear()
        {
            if (_list.Count == 0)
            {
                return;
            }

            _list.Clear();
            Notify(new ObservableListEvent<T>(ObservableListEventType.Clear, -1, default));
        }

        public void Reset(IEnumerable<T> collection)
        {
            _list.Clear();
            if (collection != null)
            {
                _list.AddRange(collection);
            }

            Notify(new ObservableListEvent<T>(ObservableListEventType.Reset, -1, default));
        }

        public void SetWithoutNotify(IEnumerable<T> collection)
        {
            _list.Clear();
            if (collection != null)
            {
                _list.AddRange(collection);
            }
        }

        public void ForceNotify()
        {
            Notify(new ObservableListEvent<T>(ObservableListEventType.Reset, -1, default));
        }

        public bool Contains(T item)
        {
            return _list.Contains(item);
        }

        public int IndexOf(T item)
        {
            return _list.IndexOf(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _list.CopyTo(array, arrayIndex);
        }

        public List<T>.Enumerator GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        public override string ToString()
        {
            return $"[{string.Join(", ", _list)}]";
        }

        private void Notify(ObservableListEvent<T> evt)
        {
            OnChanged?.Invoke(evt);
        }
    }
}
