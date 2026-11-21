using System.Linq;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Locomotion;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

namespace Dylan
{
    public class SteeringController : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] private InputActionReference enterHelm;
        [SerializeField] private InputActionReference exitHelm;

        [SerializeField] private HelmControl helm = new();

        [Header("Player")]
        [SerializeField] private XROrigin xrOrigin;
        [SerializeField] private Transform station;
        [SerializeField, Min(0f)] private float steeringHeightOffset = 0.7f;
        [SerializeField] private TrackedPoseDriver leftController;
        [SerializeField] private TrackedPoseDriver rightController;
        [SerializeField] private Transform leftVisual;
        [SerializeField] private Transform rightVisual;

        public bool IsSteering { get; private set; }
        public float WheelAngle => helm.WheelAngle;
        public float SteeringInput => helm.SteeringInput;

        private XRBaseInteractor[] handInteractors;
        private GameObject[] interactorObjects;
        private Behaviour[] suspendedBehaviours;
        private bool[] interactorWasActive;
        private bool[] behaviourWasEnabled;

        void Awake()
        {
            helm.Initialize();
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
            if (IsSteering) helm.UpdateWheel(leftController, rightController);
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

            helm.Begin(leftVisual, rightVisual);
            IsSteering = true;
        }

        private void ExitHelm()
        {
            if (!IsSteering) return;

            IsSteering = false;
            helm.End();
            if (xrOrigin != null && xrOrigin.Origin != null)
                xrOrigin.Origin.transform.position -= Vector3.up * steeringHeightOffset;

            // Restore the state of the interactor objects and suspended behaviours.
            for (int i = 0; i < suspendedBehaviours.Length; i++)
                if (suspendedBehaviours[i] != null)
                    suspendedBehaviours[i].enabled = behaviourWasEnabled[i];
            for (int i = 0; i < interactorObjects.Length; i++)
                interactorObjects[i]?.SetActive(interactorWasActive[i]);
        }
    }
}
