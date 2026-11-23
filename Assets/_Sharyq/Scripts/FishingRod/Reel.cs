using UnityEngine;
using UnityEngine.Events;

namespace _Sharyq
{
    public class Reel : MonoBehaviour
    {
        [Header("References")] 
        [SerializeField]
        private Transform rodTip;

        [SerializeField] 
        private Rigidbody hook;
        [SerializeField] 
        private SpringJoint lineJoint;
        [SerializeField] 
        private LineRenderer rope;

        [Header("Reel")] 
        [SerializeField] 
        private Vector3 localAxis = Vector3.right;
        [SerializeField] 
        private float metersPerDegree = 0.004f;
        [SerializeField] 
        private bool reelPositiveDirection = true;
        [SerializeField] 
        private float minLength = 0.15f;
        
        public UnityEvent onReeledIn;

        private float lineLength;
        private float castLength;
        private Vector3 prevRef;
        private bool isReeledIn = true;

        private void Awake()
        {
            prevRef = GetReference();
            if (isReeledIn) SnapHookHome();
        }

        private void Update()
        {
            if (!isReeledIn) UpdateReel();
            UpdateRope();
        }

        private void UpdateReel()
        {
            Vector3 axis = transform.TransformDirection(localAxis).normalized;
            Vector3 current = GetReference();
            float delta = Vector3.SignedAngle(prevRef, current, axis);
            prevRef = current;

            if (!reelPositiveDirection) delta = -delta;
            if (delta <= 0f) return;

            lineLength = Mathf.Max(minLength, lineLength - delta * metersPerDegree);
            lineJoint.maxDistance = lineLength;

            if (lineLength <= minLength + 0.001f)
                ReeledIn();
        }

        private Vector3 GetReference()
        {
            Vector3 perp = Vector3.Cross(localAxis, Vector3.up);
            if (perp.sqrMagnitude < 0.01f) perp = Vector3.Cross(localAxis, Vector3.forward);
            return transform.TransformDirection(perp.normalized);
        }

        private void ReeledIn()
        {
            isReeledIn = true;
            SnapHookHome();
            onReeledIn?.Invoke();
        }

        private void SnapHookHome()
        {
            lineJoint.maxDistance = minLength;
            hook.linearVelocity = Vector3.zero;
            hook.angularVelocity = Vector3.zero;
            hook.isKinematic = true;
            hook.transform.SetPositionAndRotation(rodTip.position, rodTip.rotation);
            hook.transform.SetParent(rodTip, true);
        }

        public void OnCast(Vector3 releaseVelocity, float maxLineLength)
        {
            isReeledIn = false;
            castLength = maxLineLength;
            lineLength = castLength;

            hook.transform.SetParent(null, true);
            hook.isKinematic = false;
            lineJoint.maxDistance = lineLength;
            hook.linearVelocity = releaseVelocity;

            prevRef = GetReference();
        }

        private void UpdateRope()
        {
            if (!rope) return;
            rope.positionCount = 2;
            rope.SetPosition(0, rodTip.position);
            rope.SetPosition(1, hook.position);
        }
    }
}