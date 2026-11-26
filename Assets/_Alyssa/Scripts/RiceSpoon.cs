using System.Reflection;
using UnityEngine;
using UnityEngine.Events;

namespace Alyssa
{
    public class RiceSpoon : MonoBehaviour
    {
        public GameObject scoopVisual;
        public UnityEvent onLoaded;
        public UnityEvent onDeposited;

        bool loaded;
        Component grab;
        PropertyInfo selected;

        public bool Loaded { get { return loaded; } }

        void Awake()
        {
            // Looked up by name so this compiles on any XR Interaction Toolkit version
            foreach (MonoBehaviour m in GetComponents<MonoBehaviour>())
            {
                if (m != null && m.GetType().Name == "XRGrabInteractable")
                {
                    grab = m;
                    selected = m.GetType().GetProperty("isSelected");
                }
            }

            SetLoaded(false);
        }

        bool IsHeld()
        {
            return grab != null && selected != null && (bool)selected.GetValue(grab);
        }

        void OnTriggerStay(Collider other)
        {
            if (loaded || !IsHeld()) return;

            RiceBowl bowl = other.GetComponentInParent<RiceBowl>();
            if (bowl != null && bowl.TryScoop())
            {
                SetLoaded(true);
                onLoaded.Invoke();
            }
        }

        void OnTriggerEnter(Collider other)
        {
            if (!loaded) return;

            RiceTarget target = other.GetComponentInParent<RiceTarget>();
            if (target != null && target.TrySpread())
            {
                SetLoaded(false);
                onDeposited.Invoke();
            }
        }

        void SetLoaded(bool value)
        {
            loaded = value;
            if (scoopVisual != null) scoopVisual.SetActive(value);
        }
    }
}