using UnityEngine;
using System;

namespace Shared{
    [CreateAssetMenu(fileName = "Resource", menuName = "Scriptable Objects/Resource")]
    public class Resource : ScriptableObject
    {
        [SerializeField] private float initialValue;
        [NonSerialized] private float runtimeValue;
        [SerializeField] public float minValue = 0f;
        [SerializeField] private GameObject prefab;

        public GameObject Prefab => prefab;
        public event Action<float> OnResourceChanged;

        public float Value
        {
            get => runtimeValue;
            set
            {
                float v = Mathf.Max(value, minValue);
                if (runtimeValue == v) return;

                runtimeValue = v;
                OnResourceChanged?.Invoke(runtimeValue);
            }
        }

        private void OnEnable()
        {
            hideFlags |= HideFlags.DontUnloadUnusedAsset;
            Value = initialValue;
        }

        public void ResetValue() => Value = initialValue;
    }
}
