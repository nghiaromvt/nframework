using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace NFramework
{
    [Serializable]
    public class ObservableList<T>
    {
        /// <summary>
        /// <CurList,ModifiedItems>
        /// </summary>
        public event Action<ReadOnlyCollection<T>, List<T>> OnListChanged;

        private List<T> _list = new List<T>();

        public T this[int index]
        {
            get => _list[index];
            set
            {
                if (_list[index] != null && _list[index].Equals(value))
                    return;

                _list[index] = value;
                OnListChanged?.Invoke(ReadOnlyList, new List<T> { value });
            }
        }

        public ReadOnlyCollection<T> ReadOnlyList => _list.AsReadOnly();
        public int Count => _list.Count;

        public ObservableList() { }

        public ObservableList(List<T> initList) => _list = initList;

        public void ForceSetList(List<T> list)
        {
            var tempList = _list;
            _list = list;
            OnListChanged?.Invoke(ReadOnlyList, tempList);
        }

        public void SetListWithoutNotify(List<T> list) => _list = list;

        public bool Contains(T value) => _list.Contains(value);

        public void Clear()
        {
            var tempList = new List<T>(_list);
            _list.Clear();
            OnListChanged?.Invoke(ReadOnlyList, tempList);
        }

        public int IndexOf(T item) => _list.IndexOf(item);

        public void ForEach(Action<T> action) => _list.ForEach(action);

        public void Add(T item)
        {
            _list.Add(item);
            OnListChanged?.Invoke(ReadOnlyList, new List<T> { item });
        }

        public void AddRange(IEnumerable<T> collection)
        {
            foreach (var item in collection)
            {
                _list.Add(item);
            }
            OnListChanged?.Invoke(ReadOnlyList, collection.ToList());
        }

        public void Remmove(T item)
        {
            _list.Remove(item);
            OnListChanged?.Invoke(ReadOnlyList, new List<T> { item });
        }

        public void RemoveAt(int index)
        {
            var item = _list[index];
            _list.RemoveAt(index);
            OnListChanged?.Invoke(ReadOnlyList, new List<T> { item });
        }

        public void RemoveRange(int index, int count)
        {
            var modifiedItems = new List<T>();
            for (int i = index; i < index + count; i++)
            {
                modifiedItems.Add(_list[i]);
            }

            _list.RemoveRange(index, Count);
            OnListChanged?.Invoke(ReadOnlyList, modifiedItems);
        }

        public void RemoveAll(Predicate<T> predicate)
        {
            var tempList = new List<T>(_list);
            var modifiedItems = new List<T>();
            foreach (var item in tempList)
            {
                if (predicate(item))
                {
                    _list.Remove(item);
                    modifiedItems.Add(item);
                }
            }
            OnListChanged?.Invoke(ReadOnlyList, modifiedItems);
        }

        public void Insert(int index, T item)
        {
            _list.Insert(index, item);
            OnListChanged?.Invoke(ReadOnlyList, new List<T> { item });
        }

        public void InsertRange(int index, IEnumerable<T> collection)
        {
            _list.InsertRange(index, collection);
            OnListChanged?.Invoke(ReadOnlyList, collection.ToList());
        }
    }
}
