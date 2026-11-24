using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR;

namespace Dylan
{
    public sealed class HelmControl : MonoBehaviour
    {
        private const float MinimumHandSeparation = 0.12f;

        [SerializeField] private XRControls xrControls;
        [SerializeField] private Transform wheelPivot;
        [SerializeField] private Transform leftGrip;
        [SerializeField] private Transform rightGrip;
        [SerializeField, Range(1f, 180f)] private float maximumWheelAngle = 60f;

        public float WheelAngle { get; private set; }
        public float SteeringInput => WheelAngle / maximumWheelAngle;
        internal bool ExitRequested => exitHelm.triggered;

        private InputAction exitHelm;
        private Quaternion neutralWheelRotation;
        private VisualPose leftPose;
        private VisualPose rightPose;
        private bool hasReference;
        private Vector3 previousDirection;

        void Awake()
        {
            exitHelm = xrControls.HelmActions.FindAction("ExitHelm", true);
            neutralWheelRotation = wheelPivot.localRotation;
        }

        internal void Begin(Transform leftVisual, Transform rightVisual)
        {
            leftPose = new VisualPose(leftVisual, leftGrip);
            rightPose = new VisualPose(rightVisual, rightGrip);
            hasReference = false;
        }

        internal void UpdateWheel(TrackedPoseDriver leftController, TrackedPoseDriver rightController)
        {
            if (!TryReadPosition(leftController, out var leftPosition) ||
                !TryReadPosition(rightController, out var rightPosition))
            {
                hasReference = false;
                return;
            }

            var separation = rightPosition - leftPosition;
            var direction = Quaternion.Inverse(neutralWheelRotation) *
                wheelPivot.parent.InverseTransformDirection(separation);
            direction.z = 0f;
            if (direction.sqrMagnitude < MinimumHandSeparation * MinimumHandSeparation)
            {
                hasReference = false;
                return;
            }

            if (hasReference)
            {
                float change = -Vector3.SignedAngle(previousDirection, direction, Vector3.forward);
                WheelAngle = Mathf.Clamp(WheelAngle + change, -maximumWheelAngle, maximumWheelAngle);
                wheelPivot.localRotation = neutralWheelRotation * Quaternion.AngleAxis(-WheelAngle, Vector3.forward);
            }

            previousDirection = direction;
            hasReference = true;
        }

        internal void End()
        {
            leftPose.Restore();
            rightPose.Restore();
        }

        private static bool TryReadPosition(TrackedPoseDriver controller, out Vector3 position)
        {
            if (controller.isActiveAndEnabled &&
                (controller.trackingStateInput.action.ReadValue<int>() & (int)InputTrackingState.Position) != 0)
            {
                // The tracked transform can still contain the previous pose when tracking resumes.
                position = controller.transform.parent.TransformPoint(controller.positionInput.action.ReadValue<Vector3>());
                return true;
            }

            position = default;
            return false;
        }

        private readonly struct VisualPose
        {
            private readonly Transform visual;
            private readonly Transform parent;
            private readonly Vector3 position;
            private readonly Quaternion rotation;
            private readonly Vector3 scale;
            private readonly int siblingIndex;
            private readonly bool wasActive;

            public VisualPose(Transform visual, Transform grip)
            {
                this.visual = visual;
                parent = visual.parent;
                position = visual.localPosition;
                rotation = visual.localRotation;
                scale = visual.localScale;
                siblingIndex = visual.GetSiblingIndex();
                wasActive = visual.gameObject.activeSelf;

                var offset = Quaternion.Inverse(parent.rotation) * parent.TransformVector(position);
                visual.SetParent(grip);
                visual.SetPositionAndRotation(grip.position + grip.rotation * offset, grip.rotation * rotation);
                visual.gameObject.SetActive(true);
            }

            public void Restore()
            {
                if (visual == null || parent == null) return;

                visual.SetParent(parent, false);
                visual.SetLocalPositionAndRotation(position, rotation);
                visual.localScale = scale;
                visual.SetSiblingIndex(siblingIndex);
                visual.gameObject.SetActive(wasActive);
            }
        }
    }
}
