using UnityEngine;
using UnityEngine.Events;

namespace Alyssa
{
    public class BambooMatZone : MonoBehaviour
    {
        public Transform snapPoint;
        public UnityEvent onNoriPlaced;

        void OnTriggerStay(Collider other)
        {
            Nori n = other.GetComponentInParent<Nori>();
            if (n == null || n.OnMat || n.IsHeld()) return;

            n.SnapTo(snapPoint != null ? snapPoint : transform);
            onNoriPlaced.Invoke();
        }
    }
}