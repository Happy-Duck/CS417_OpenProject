using UnityEngine;
using System;

namespace Shared{
    [CreateAssetMenu(fileName = "Stat", menuName = "Scriptable Objects/Stat")]
    public class Stat : ScriptableObject
    {
        [SerializeField] private float initialValue;
        [NonSerialized] private float runtimeValue;
        [SerializeField] public float minValue = 0f;
        [SerializeField] public float maxValue = 100f;

        public event Action<float> OnStatChanged;

        public float Value
        {
            get => runtimeValue;
            set
            {
                float v = Mathf.Clamp(value, minValue, maxValue);
                if (runtimeValue == v) return;

                runtimeValue = v;
                OnStatChanged?.Invoke(runtimeValue);
            }
        }

        private void OnEnable()
        {
            hideFlags |= HideFlags.DontUnloadUnusedAsset;
            Value = initialValue;
        }

        public void ResetValue()
        {
            Value = initialValue;
        }
    }
}
