using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Phoder1.SaveableData
{
    /// <summary>
    /// A data interface which implements the IsDirt flag design pattern.
    /// <a href="https://gpgroup13.wordpress.com/a-dirty-flag-tutorial/#consider">(Refrence)</a>
    /// Make sure to run the ValueChanged function on every property change.
    /// </summary>
    public interface IDirtyData
    {
        bool IsDirty { get; }
        void Clean();
        void ValueChanged();
        event Action OnDirty;
        event Action OnValueChange;
    }
    public abstract class DirtyData : IDirtyData
    {
        [NonSerialized]
        private bool _isDirty = false;
        public virtual bool IsDirty
        {
            get => _isDirty;
            protected set
            {
                if (IsDirty == value)
                    return;

                _isDirty = value;

                if (IsDirty)
                {
                    OnDirty?.Invoke();
                }
            }
        }
        public event Action OnDirty;
        public event Action OnValueChange;

        public void Clean()
        {
            IsDirty = false;
            OnClean();
        }

        protected virtual void OnClean() { }

        public virtual void ValueChanged()
        {
            OnValueChange?.Invoke();
            IsDirty = true;
        }
        protected void Setter<T>(ref T data, T value, Action<T> onValueChangedAction = null)
        {
            if (IsDirty && onValueChangedAction == null && OnValueChange == null)
            {
                data = value;
                return;
            }

            if ((data == null && value == null) || (data != null && data.Equals(value)))
                return;

            data = value;

            onValueChangedAction?.Invoke(data);
            ValueChanged();
        }
    }
}
