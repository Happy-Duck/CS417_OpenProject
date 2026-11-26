using UnityEngine;

namespace _Sharyq
{
    [RequireComponent(typeof(Rigidbody))]
    public class Hook : MonoBehaviour
    {
        public FishNavigation Fish;
        
        [SerializeField] private Transform rodTip;
        [SerializeField] private RopeBuilder rope;
        [SerializeField] private bool inWater;
        [SerializeField] private float waterLevel = -6f;

        private Rigidbody rb;
        private float lockedDistance;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
        }

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
            if (rodTip == null) return;
            
            if (rope != null)
                lockedDistance = Mathf.Min(lockedDistance, rope.CurrentLength);

            Vector3 tip = rodTip.position;
            Vector3 pos = rb.position;
            pos.y = waterLevel;

            Vector3 flat = new Vector3(pos.x - tip.x, 0f, pos.z - tip.z);
            if (flat.sqrMagnitude < 0.0001f)
            {
                flat = Vector3.ProjectOnPlane(rodTip.forward, Vector3.up);
                if (flat.sqrMagnitude < 0.0001f) flat = Vector3.forward;
            }
            flat.Normalize();

            float dy = waterLevel - tip.y;
            float radiusSqr = lockedDistance * lockedDistance - dy * dy;
            float radius = radiusSqr > 0f ? Mathf.Sqrt(radiusSqr) : 0f;

            pos.x = tip.x + flat.x * radius;
            pos.z = tip.z + flat.z * radius;

            rb.position = pos;
            if (!rb.isKinematic)
                rb.linearVelocity = Vector3.zero;
        }
    }
}