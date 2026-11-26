using System.Collections;
using System.Reflection;
using UnityEngine;

namespace Alyssa
{
    public class RollIngredient : MonoBehaviour
    {
        public IngredientData ingredient;

        Component grab;
        PropertyInfo selected;

        public bool Placed { get; private set; }

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
            Placed = true;

            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = true;

            // Colliders off so it isn't re-detected and doesn't block the spoon or the nori's grab
            foreach (Collider c in GetComponentsInChildren<Collider>()) c.enabled = false;

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