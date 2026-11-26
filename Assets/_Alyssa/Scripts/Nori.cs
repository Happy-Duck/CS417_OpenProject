using System.Reflection;
using UnityEngine;

namespace Alyssa
{
    [RequireComponent(typeof(Rigidbody))]
    public class Nori : MonoBehaviour
    {
        Rigidbody rb;
        Component grab;
        PropertyInfo selected;

        public bool OnMat { get; private set; }

        void Awake()
        {
            rb = GetComponent<Rigidbody>();

            // Same name lookup as RiceSpoon, works on any XRI version
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

        void Update()
        {
            // Picked back up off the mat -> unlock it
            if (OnMat && IsHeld())
            {
                OnMat = false;
                rb.constraints = RigidbodyConstraints.None;
            }
        }

        public void SnapTo(Transform point)
        {
            rb.linearVelocity = Vector3.zero;      // use rb.velocity on older Unity versions
            rb.angularVelocity = Vector3.zero;
            transform.SetPositionAndRotation(point.position, point.rotation);
            rb.constraints = RigidbodyConstraints.FreezeAll;
            OnMat = true;
        }
    }
}