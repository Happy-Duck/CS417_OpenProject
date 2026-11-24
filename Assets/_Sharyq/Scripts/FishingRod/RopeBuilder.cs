using UnityEngine;

namespace _Sharyq
{
    [RequireComponent(typeof(LineRenderer))]
    public class RopeBuilder : MonoBehaviour
    {
        [Header("Rod Objects")]
        [SerializeField]
        private Rigidbody anchor;
        [SerializeField]
        private Transform anchorPoint;
        [SerializeField]
        private Rigidbody hook;

        [Header("Rope Settings")]
        [SerializeField]
        private int segments = 12;
        [SerializeField]
        private float ropeLength = 1.0f;
        [SerializeField]
        private float segmentMass = 0.02f;
        [SerializeField]
        private float damping = 0.5f;
        [SerializeField]
        private int solverIterations = 20;
        [SerializeField]
        private Vector3 heldLocalDirection = new Vector3(0f, -1f, 0.3f);

        private const float HookOffset = 0.05f;

        private Rigidbody[] bodies;
        private LineRenderer lineRenderer;
        private float segLen;
        private bool isHeld;

        public bool IsHeld => isHeld;

        private void Start()
        {
            // Init
            lineRenderer = GetComponent<LineRenderer>();
            bodies = new Rigidbody[segments];
            segLen = ropeLength / segments;

            Vector3 dir = anchorPoint.TransformDirection(heldLocalDirection).normalized;
            Rigidbody prev = anchor;
            Vector3 pos = anchorPoint.position;

            // Chain Building
            for (int i = 0; i < segments; i++)
            {
                pos += dir * segLen;

                var go = new GameObject($"RopeSeg_{i}");
                go.transform.position = pos;
                go.transform.parent = null;

                var rb = go.AddComponent<Rigidbody>();
                rb.mass = segmentMass;
                rb.linearDamping = damping;
                rb.angularDamping = damping;
                rb.solverIterations = solverIterations;

                var joint = go.AddComponent<ConfigurableJoint>();
                joint.connectedBody = prev;
                joint.autoConfigureConnectedAnchor = false;
                joint.anchor = Vector3.zero;
                joint.connectedAnchor = (i == 0)
                    ? anchor.transform.InverseTransformPoint(anchorPoint.position)
                    : Vector3.zero;

                joint.xMotion = joint.yMotion = joint.zMotion = ConfigurableJointMotion.Limited;
                joint.linearLimit = new SoftJointLimit { limit = segLen };
                joint.angularXMotion = joint.angularYMotion = joint.angularZMotion = ConfigurableJointMotion.Free;
                joint.enablePreprocessing = false;

                bodies[i] = rb;
                prev = rb;
            }

            // place hook
            hook.transform.position = pos + dir * HookOffset;
            hook.linearVelocity = Vector3.zero;
            hook.angularVelocity = Vector3.zero;

            var hj = hook.gameObject.AddComponent<ConfigurableJoint>();
            hj.connectedBody = prev;
            hj.xMotion = hj.yMotion = hj.zMotion = ConfigurableJointMotion.Locked;
            hj.angularXMotion = hj.angularYMotion = hj.angularZMotion = ConfigurableJointMotion.Free;
            hj.enablePreprocessing = false;

            // hard cap on total line length (hook can never get farther than this from the rod tip)
            AddLengthCap();

            // hook wont hit rod
            var hookCols = hook.GetComponentsInChildren<Collider>();
            var rodCols = anchor.GetComponentsInChildren<Collider>();
            foreach (var h in hookCols)
            foreach (var r in rodCols)
                Physics.IgnoreCollision(h, r);

            lineRenderer.positionCount = segments + 2;

            hook.maxLinearVelocity = 15f;
            hook.solverIterations = solverIterations;
            hook.interpolation = RigidbodyInterpolation.Interpolate;
            hook.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            // start held
            SetHeld(true);
        }

        // the hook can never be farther than the rope length from the rod tip
        private void AddLengthCap()
        {
            var cap = hook.gameObject.AddComponent<ConfigurableJoint>();
            cap.connectedBody = anchor;
            cap.autoConfigureConnectedAnchor = false;
            cap.anchor = Vector3.zero;
            cap.connectedAnchor = anchor.transform.InverseTransformPoint(anchorPoint.position);

            cap.xMotion = cap.yMotion = cap.zMotion = ConfigurableJointMotion.Limited;
            cap.linearLimit = new SoftJointLimit { limit = ropeLength + HookOffset, bounciness = 0f };
            cap.angularXMotion = cap.angularYMotion = cap.angularZMotion = ConfigurableJointMotion.Free;
            cap.enablePreprocessing = false;
        }

        private void FixedUpdate()
        {
            if (isHeld) LayoutHeld();
        }

        // fixed length, while held out
        private void LayoutHeld()
        {
            Vector3 dir = anchorPoint.TransformDirection(heldLocalDirection).normalized;
            Vector3 start = anchorPoint.position;

            for (int i = 0; i < bodies.Length; i++)
                bodies[i].position = start + dir * (segLen * (i + 1));

            hook.position = start + dir * (ropeLength + HookOffset);
            hook.rotation = Quaternion.LookRotation(dir);
        }

        public void SetHeld(bool held)
        {
            isHeld = held;

            foreach (var b in bodies) ApplyState(b, held);
            ApplyState(hook, held);

            if (held) LayoutHeld();
        }

        private static void ApplyState(Rigidbody rb, bool held)
        {
            if (held)
            {
                if (!rb.isKinematic)
                {
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }
                rb.isKinematic = true;
            }
            else
            {
                rb.isKinematic = false;
            }
        }
        
        public void Cast(Vector3 velocity)
        {
            if (!isHeld) return;
            SetHeld(false);
            
            foreach (var body in bodies) body.linearVelocity = velocity;
            hook.linearVelocity = velocity;
        }

        //retrieve
        public void ResetToHeld() => SetHeld(true);

        private void LateUpdate()
        {
            if (bodies == null) return;

            // use transform.position (not rb.position) so interpolation is respected
            lineRenderer.SetPosition(0, anchorPoint.position);
            for (int i = 0; i < bodies.Length; i++)
                lineRenderer.SetPosition(i + 1, bodies[i].transform.position);
            lineRenderer.SetPosition(bodies.Length + 1, hook.transform.position);
        }
    }
}