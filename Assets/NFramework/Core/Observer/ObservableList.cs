using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace NFramework
{
    public class ObservableList<T>
    {
        /// <summary>
        /// Event triggered when the list is modified.
        /// Parameters: Current list (read-only), Modified items.
        /// </summary>
        public event Action<ReadOnlyCollection<T>, List<T>> OnListChanged;

        private List<T> _list = new List<T>();

        public T this[int index]
        {
            get => _list[index];
            set
            {
                if (EqualityComparer<T>.Default.Equals(_list[index], value))
                    return;

                _list[index] = value;
                Notify(new List<T> { value });
            }
        }

        public ReadOnlyCollection<T> ReadOnlyList => _list.AsReadOnly();
        public int Count => _list.Count;

        public ObservableList() { }

        public ObservableList(IEnumerable<T> initList) => _list = initList.ToList();

        public void ForceSetList(List<T> list)
        {
            var previous = _list;
            _list = list;
            Notify(previous);
        }

        /// <summary>
        /// Replace the list without triggering the OnListChanged event. Use with care.
        /// </summary>
        public void SetListWithoutNotify(List<T> list) => _list = list;

        public bool Contains(T value) => _list.Contains(value);
        public int IndexOf(T item) => _list.IndexOf(item);
        public void ForEach(Action<T> action) => _list.ForEach(action);

        public void Add(T item)
        {
            _list.Add(item);
            Notify(new List<T> { item });
        }

        public void AddRange(IEnumerable<T> collection)
        {
            var items = collection.ToList();
            _list.AddRange(items);
            Notify(items);
        }

        public void Remove(T item)
        {
            if (_list.Remove(item))
                Notify(new List<T> { item });
        }

        public void RemoveAt(int index)
        {
            var item = _list[index];
            _list.RemoveAt(index);
            Notify(new List<T> { item });
        }

        public void RemoveRange(int index, int count)
        {
            var modifiedItems = _list.Skip(index).Take(count).ToList();
            _list.RemoveRange(index, count);
            Notify(modifiedItems);
        }

        public void RemoveAll(Predicate<T> predicate)
        {
            var removedItems = _list.Where(item => predicate(item)).ToList();
            _list.RemoveAll(predicate);
            Notify(removedItems);
        }

        public void Insert(int index, T item)
        {
            _list.Insert(index, item);
            Notify(new List<T> { item });
        }

        public void InsertRange(int index, IEnumerable<T> collection)
        {
            var items = collection.ToList();
            _list.InsertRange(index, items);
            Notify(items);
        }

        public void Clear()
        {
            var tempList = new List<T>(_list);
            _list.Clear();
            Notify(tempList);
        }

        private void Notify(List<T> modifiedItems)
        {
            OnListChanged?.Invoke(ReadOnlyList, modifiedItems);
        }

        public override string ToString() => $"[{string.Join(", ", _list)}]";

        public static implicit operator ReadOnlyCollection<T>(ObservableList<T> observable) => observable.ReadOnlyList;
    }
}
