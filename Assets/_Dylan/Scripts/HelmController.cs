using System.Linq;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Locomotion;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

namespace Dylan
{
    public class HelmController : MonoBehaviour
    {
        private const float MinimumHandSeparation = 0.12f;

        [Header("Input")]
        [SerializeField] private InputActionReference enterHelm;
        [SerializeField] private InputActionReference exitHelm;

        [Header("Wheel")]
        [SerializeField] private Transform wheelPivot;
        [SerializeField] private Transform leftGrip;
        [SerializeField] private Transform rightGrip;
        [SerializeField, Range(1f, 180f)] private float maximumWheelAngle = 60f;

        [Header("Player")]
        [SerializeField] private XROrigin xrOrigin;
        [SerializeField] private Transform station;
        [SerializeField, Min(0f)] private float steeringHeightOffset = 0.7f;
        [SerializeField] private TrackedPoseDriver leftController;
        [SerializeField] private TrackedPoseDriver rightController;
        [SerializeField] private Transform leftVisual;
        [SerializeField] private Transform rightVisual;

        public bool IsSteering { get; private set; }
        public float WheelAngle { get; private set; }
        public float SteeringInput => WheelAngle / maximumWheelAngle;

        private Quaternion neutralWheelRotation;
        private XRBaseInteractor[] handInteractors;
        private GameObject[] interactorObjects;
        private Behaviour[] suspendedBehaviours;
        private bool[] interactorWasActive;
        private bool[] behaviourWasEnabled;
        private VisualPose leftPose;
        private VisualPose rightPose;
        private bool hasReference;
        private Vector3 previousDirection;

        void Awake()
        {
            neutralWheelRotation = wheelPivot.localRotation;
            handInteractors = 
                leftController.GetComponentsInChildren<XRBaseInteractor>(true)
                .Concat(rightController.GetComponentsInChildren<XRBaseInteractor>(true)).ToArray();
            interactorObjects = 
                xrOrigin.GetComponentsInChildren<XRBaseInteractor>(true)
                .Select(interactor => interactor.gameObject).Distinct().ToArray();
            suspendedBehaviours = 
                xrOrigin.GetComponentsInChildren<ControllerInputActionManager>(true)
                .Cast<Behaviour>()
                .Concat(xrOrigin.GetComponentsInChildren<LocomotionProvider>(true)).ToArray();
            interactorWasActive = new bool[interactorObjects.Length];
            behaviourWasEnabled = new bool[suspendedBehaviours.Length];
        }

        void OnEnable()
        {
            enterHelm.action.Enable();
            exitHelm.action.Enable();
        }

        void OnDisable()
        {
            enterHelm.action.Disable();
            exitHelm.action.Disable();
            ExitHelm();
        }

        void Update()
        {
            if (!IsSteering && enterHelm.action.triggered) TryEnterHelm();
            else if (IsSteering && exitHelm.action.triggered) ExitHelm();
        }

        void LateUpdate()
        {
            if (IsSteering) UpdateWheel();
        }

        private void TryEnterHelm()
        {
            // return if any hand interactor is currently selecting an object.
            if (handInteractors.Any(interactor => interactor.hasSelection)) return;

            var head = xrOrigin.Camera.transform;

            // Store the current state of the interactor objects and suspended behaviours.
            for (int i = 0; i < interactorObjects.Length; i++)
                interactorWasActive[i] = interactorObjects[i].activeSelf;
            for (int i = 0; i < suspendedBehaviours.Length; i++)
                behaviourWasEnabled[i] = suspendedBehaviours[i].enabled;

            // Disable the interactor objects and suspended behaviours.
            foreach (var behaviour in suspendedBehaviours)
                behaviour.enabled = false;
            foreach (var interactor in interactorObjects)
                interactor.SetActive(false);

            // Teleport the player to the helm station and rotate them to face forward.
            xrOrigin.RotateAroundCameraUsingOriginUp(
                Mathf.DeltaAngle(head.eulerAngles.y, station.eulerAngles.y)
            );
            xrOrigin.Origin.transform.position +=
                Vector3.ProjectOnPlane(station.position - head.position, Vector3.up) +
                Vector3.up * steeringHeightOffset;

            leftPose = new VisualPose(leftVisual, leftGrip);
            rightPose = new VisualPose(rightVisual, rightGrip);
            hasReference = false;
            IsSteering = true;
        }

        private void ExitHelm()
        {
            if (!IsSteering) return;

            IsSteering = false;
            if (xrOrigin != null && xrOrigin.Origin != null)
                xrOrigin.Origin.transform.position -= Vector3.up * steeringHeightOffset;

            leftPose.Restore();
            rightPose.Restore();

            // Restore the state of the interactor objects and suspended behaviours.
            for (int i = 0; i < suspendedBehaviours.Length; i++)
                if (suspendedBehaviours[i] != null) 
                    suspendedBehaviours[i].enabled = behaviourWasEnabled[i];
            for (int i = 0; i < interactorObjects.Length; i++)
                interactorObjects[i]?.SetActive(interactorWasActive[i]);
        }

        private void UpdateWheel()
        {
            // check if both controllers have a tracked position.
            if (!HasTrackedPosition(leftController) || !HasTrackedPosition(rightController))
            {
                hasReference = false;
                return;
            }

            // a vector form left controller to right controller.
            var separation = rightController.transform.position - leftController.transform.position;
            // calaculate the angle of the separation vector in the local space of the wheel pivot.
            var direction = Quaternion.Inverse(neutralWheelRotation) * transform.InverseTransformDirection(separation);
            // remove the z component of the direction vetor.
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

        // Check whether the given Controller has a tracked position.
        private static bool HasTrackedPosition(TrackedPoseDriver controller)
        {
            return 
                controller.isActiveAndEnabled &&
                (controller.trackingStateInput.action.ReadValue<int>() & 
                (int)InputTrackingState.Position) != 0;
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
