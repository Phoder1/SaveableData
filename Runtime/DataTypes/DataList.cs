using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Phoder1.SaveableData
{
    [Serializable]
    public abstract class BaseDirtyList<T> : DirtyData, ICollection<T>, IEnumerable<T>, IEnumerable, IList<T>, IReadOnlyCollection<T>, IReadOnlyList<T>
    {
        public List<T> collection = new List<T>();
        public T this[int index]
        {
            get => collection[index];
            set
            {
                if ((collection[index] == null && value == null) || (collection[index] != null && collection[index].Equals(value)))
                    return;

                collection[index] = value;

                ValueChanged();
            }
        }
        public int Count => collection.Count;
        public bool IsReadOnly => false;
        public virtual void Add(T item)
        {
            collection.Add(item);
            ValueChanged();
        }
        public virtual void Clear()
        {
            if (collection.Count != 0)
            {
                collection.Clear();

                ValueChanged();
            }
        }
        public bool Contains(T item) => collection.Contains(item);
        public void CopyTo(T[] array, int arrayIndex) => collection.CopyTo(array, arrayIndex);

        public int IndexOf(T item) => collection.IndexOf(item);
        public T Find(Predicate<T> match) => collection.Find(match);
        public T FindLast(Predicate<T> match) => collection.FindLast(match);
        public int FindLastIndex(int startIndex, int count, Predicate<T> match) => collection.FindLastIndex(startIndex, count, match);
        public int FindLastIndex(int startIndex, Predicate<T> match) => collection.FindLastIndex(startIndex, match);
        public int FindLastIndex(Predicate<T> match) => collection.FindLastIndex(match);

        public virtual void Insert(int index, T item)
        {
            collection.Insert(index, item);
            ValueChanged();
        }

        public virtual bool Remove(T item)
        {
            if (collection.Remove(item))
            {
                ValueChanged();
                return true;
            }
            return false;
        }

        public virtual void RemoveAt(int index)
        {
            collection.RemoveAt(index);
            ValueChanged();
        }
        public int FindIndex(int startIndex, int count, Predicate<T> match) => collection.FindIndex(startIndex, count, match);
        public int FindIndex(int startIndex, Predicate<T> match) => collection.FindIndex(startIndex, match);
        public int FindIndex(Predicate<T> match) => collection.FindIndex(match);
        public void Sort(Comparison<T> comparison)
        {
            collection.Sort(comparison);

            ValueChanged();
        }
        public void Sort(int index, int count, IComparer<T> comparer)
        {
            collection.Sort(index, count, comparer);

            ValueChanged();
        }
        public void Sort()
        {
            collection.Sort();

            ValueChanged();
        }
        public void Sort(IComparer<T> comparer)
        {
            collection.Sort(comparer);

            ValueChanged();
        }
        public IEnumerator<T> GetEnumerator() => collection.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
    [Serializable]
    public class DirtyDataList<T> : BaseDirtyList<T>, ISerializationCallbackReceiver where T : IDirtyData
    {
        public DirtyDataList()
        {
            collection.ForEach((x) => x.OnValueChange += ValueChanged);
        }

        public override bool IsDirty
        {
            get => base.IsDirty || collection.Exists((X) => X.IsDirty);
            protected set => base.IsDirty = value;
        }

        public override void Add(T item)
        {
            base.Add(item);
            item.OnValueChange += ValueChanged;
        }

        public override void Clear()
        {
            collection.ForEach((x) => x.OnValueChange -= ValueChanged);

            base.Clear();
        }

        public override void Insert(int index, T item)
        {
            base.Insert(index, item);
            item.OnValueChange += ValueChanged;
        }

        public void OnAfterDeserialize()
        {
            collection.ForEach((x) => x.OnValueChange += ValueChanged);
        }

        public void OnBeforeSerialize()
        {
            collection.ForEach((x) => x.OnValueChange -= ValueChanged);
        }

        public override bool Remove(T item)
        {
            item.OnValueChange -= ValueChanged;

            return base.Remove(item);
        }

        public override void RemoveAt(int index)
        {
            collection[index].OnValueChange -= ValueChanged;

            base.RemoveAt(index);
        }

        protected override void OnClean()
        {
            base.OnClean();
            collection.ForEach((X) => X.Clean());
        }

    }
    [Serializable]
    public class DirtyStructList<T> : BaseDirtyList<T> where T : struct { }
}
