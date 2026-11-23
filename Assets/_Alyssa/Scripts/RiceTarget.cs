using UnityEngine;
using UnityEngine.Events;

namespace Alyssa
{
    public class RiceTarget : MonoBehaviour
    {
        public GameObject spreadRicePrefab;
        public Transform spreadPoint;
        public Freshness decayOnRice;
        public UnityEvent onSpread;
        public UnityEvent onRejected;

        bool hasRice;

        public bool HasRice { get { return hasRice; } }

        public bool TrySpread()
        {
            if (hasRice)
            {
                onRejected.Invoke();
                return false;
            }

            hasRice = true;

            Transform point = spreadPoint != null ? spreadPoint : transform;
            Instantiate(spreadRicePrefab, point.position, point.rotation, transform);

            if (decayOnRice != null) decayOnRice.isDecaying = true;
            onSpread.Invoke();
            return true;
        }
    }
}