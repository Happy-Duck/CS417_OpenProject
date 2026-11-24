using UnityEngine;

namespace _Sharyq
{
    [RequireComponent(typeof(Rigidbody))]
    public class Hook : MonoBehaviour
    {
        [SerializeField] private Transform rodTip;
        [SerializeField] private bool inWater;
        [SerializeField] private float waterLevel = -6f;

        private Rigidbody rb;
        private float lockedDistance;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
        }

        public void SetRodTip(Transform tip) => rodTip = tip;

        private void FixedUpdate()
        {
            if (!inWater)
            {
                if (rb.position.y <= waterLevel)
                    EnterWater();
                return;
            }

            LockToSurface();
        }

        private void EnterWater()
        {
            inWater = true;

            // Remember how far the hook is from the rod tip at the moment it hits the water
            lockedDistance = rodTip != null
                ? Vector3.Distance(rodTip.position, rb.position)
                : 0f;

            rb.useGravity = false;
            if (!rb.isKinematic)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }

        public void LeaveWater()
        {
            inWater = false;
            rb.useGravity = true;
        }

        private void LockToSurface()
        {
            Vector3 pos = rb.position;
            pos.y = waterLevel;

            if (rodTip != null)
            {
                Vector3 tip = rodTip.position;

                // Horizontal direction from rod tip to hook
                Vector3 flat = new Vector3(pos.x - tip.x, 0f, pos.z - tip.z);
                if (flat.sqrMagnitude < 0.0001f)
                {
                    flat = Vector3.ProjectOnPlane(rodTip.forward, Vector3.up);
                    if (flat.sqrMagnitude < 0.0001f) flat = Vector3.forward;
                }
                flat.Normalize();

                // Keep the true 3D distance equal to lockedDistance while sitting at water level
                float dy = waterLevel - tip.y;
                float radiusSqr = lockedDistance * lockedDistance - dy * dy;
                float radius = radiusSqr > 0f ? Mathf.Sqrt(radiusSqr) : 0f;

                pos.x = tip.x + flat.x * radius;
                pos.z = tip.z + flat.z * radius;
            }

            rb.position = pos;
            if (!rb.isKinematic)
                rb.linearVelocity = Vector3.zero;
        }
    }
}