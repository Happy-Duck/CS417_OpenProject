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
        private Vector3 spawnDirection = new Vector3(0f, -1f, 0.3f);

        private Rigidbody[] _bodies;
        private LineRenderer _lineRenderer;

        private void Start()
        {
            // Init
            _lineRenderer = GetComponent<LineRenderer>();
            _bodies = new Rigidbody[segments];

            float segLen = ropeLength / segments;
            Vector3 dir = spawnDirection.normalized;
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
                
                _bodies[i] = rb;
                prev = rb;
            }

            // place hook
            hook.transform.position = pos + dir * 0.05f;
            hook.linearVelocity = Vector3.zero;

            var hj = hook.gameObject.AddComponent<ConfigurableJoint>();
            hj.connectedBody = prev;
            hj.xMotion = hj.yMotion = hj.zMotion = ConfigurableJointMotion.Locked;
            hj.angularXMotion = hj.angularYMotion = hj.angularZMotion = ConfigurableJointMotion.Free;
            hj.enablePreprocessing = false;

            // hook wont hit rod
            var hookCols = hook.GetComponentsInChildren<Collider>();
            var rodCols = anchor.GetComponentsInChildren<Collider>();
            foreach (var h in hookCols)
            foreach (var r in rodCols)
                Physics.IgnoreCollision(h, r);

            _lineRenderer.positionCount = segments + 2;
            
            hook.maxLinearVelocity = 15f;
        }

        void LateUpdate()
        {
            if (_bodies == null) return;

            _lineRenderer.SetPosition(0, anchorPoint.position);
            for (int i = 0; i < _bodies.Length; i++)
                _lineRenderer.SetPosition(i + 1, _bodies[i].position);
            _lineRenderer.SetPosition(_bodies.Length + 1, hook.position);
        }
    }
}