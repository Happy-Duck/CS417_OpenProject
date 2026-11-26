using UnityEngine;
using UnityEngine.Events;

namespace _Sharyq
{
    public class Reel : MonoBehaviour
    {
        [SerializeField] RopeBuilder rope;
        [SerializeField] Vector3 localAxis = Vector3.right;
        [SerializeField] float metersPerDegree = 0.004f;
        [SerializeField] bool reelPositiveDirection = true;

        public UnityEvent onReeledIn;

        Vector3 prevRef;

        void OnEnable()
        {
            prevRef = GetReference();
        }

        void Update()
        {
            Vector3 axis = transform.localRotation * localAxis;
            Vector3 current = GetReference();
            float delta = Vector3.SignedAngle(prevRef, current, axis);
            prevRef = current;

            if (rope == null || rope.IsHeld) return;

            if (!reelPositiveDirection) delta = -delta;
            if (delta <= 0f) return;

            rope.ReelTo(rope.CurrentLength - delta * metersPerDegree);

            if (rope.IsHeld) onReeledIn?.Invoke();
        }

        Vector3 GetReference()
        {
            Vector3 perp = Vector3.Cross(localAxis, Vector3.up);
            if (perp.sqrMagnitude < 0.01f) perp = Vector3.Cross(localAxis, Vector3.forward);
            return transform.localRotation * perp.normalized;
        }
    }
}