using UnityEngine;
using System;

namespace Shared{
    [CreateAssetMenu(fileName = "Resource", menuName = "Scriptable Objects/Resource")]
    public class Resource : ScriptableObject
    {
        [SerializeField] private float initialValue;
        [NonSerialized] private float runtimeValue;
        [SerializeField] public float minValue = 0f;
        [SerializeField] public float maxValue = 100f;
        [SerializeField] private GameObject prefab;

        public GameObject Prefab => prefab;
        public event Action<float> OnResourceChanged;

        public float Value
        {
            get => runtimeValue;
            set
            {
                float clamped = Mathf.Clamp(value, minValue, maxValue);
                if (runtimeValue == clamped) return;

                runtimeValue = clamped;
                OnResourceChanged?.Invoke(runtimeValue);
            }
        }

        private void OnEnable()
        {
            Value = initialValue;
        }
    }
}
