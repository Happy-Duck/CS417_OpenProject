using System.Collections;
using System.Reflection;
using UnityEngine;

namespace Alyssa
{
    public class PlateItem : MonoBehaviour
    {
        public string itemName = "Tuna Sashimi";

        [HideInInspector] public bool plated;

        Component grab;
        PropertyInfo selected;

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
        }

        public bool IsHeld()
        {
            return grab != null && selected != null && (bool)selected.GetValue(grab);
        }

        public void Lock(Transform slot)
        {
            plated = true;

            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = true;

            transform.SetPositionAndRotation(slot.position, slot.rotation);
            transform.SetParent(slot, true);

            StartCoroutine(StripPhysics(rb));
        }

        IEnumerator StripPhysics(Rigidbody rb)
        {
            foreach (MonoBehaviour m in GetComponents<MonoBehaviour>())
                if (m != null && m.GetType().Name.StartsWith("XR")) Destroy(m);

            // The grab component requires the Rigidbody, so it goes one frame later
            yield return null;
            if (rb != null) Destroy(rb);
        }
    }
}