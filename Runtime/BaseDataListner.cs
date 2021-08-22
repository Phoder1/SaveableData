using System;
using UnityEngine;
using UnityEngine.Events;

namespace Phoder1.SaveableData
{
    public abstract class BaseDataListner<TData> : MonoBehaviour 
        where TData : DirtyData, new()
    {
        [SerializeField]
        private SaveableEvent OnLoad;
        [SerializeField]
        private SaveableEvent OnValueChanged;
        protected abstract TData Saveable { get; }
        protected virtual void OnEnable()
        {
            OnLoad?.Invoke(Saveable);
            Saveable.OnValueChange += ValuChanged;
        }
        protected virtual void OnDisable()
        {
            Saveable.OnValueChange -= ValuChanged;
        }
        private void ValuChanged()
        {
            OnValueChanged?.Invoke(Saveable);
        }

        [Serializable]
        public class SaveableEvent : UnityEvent<TData> { }
    }
}
