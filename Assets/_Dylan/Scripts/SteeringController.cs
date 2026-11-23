using System.Linq;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactors.Casters;

namespace Dylan
{
    public class SteeringController : MonoBehaviour
    {
        [SerializeField] private HelmControl helm;
        [SerializeField] private ThrottleControl throttle;
        [SerializeField] private GameObject steeringHUD;
        [SerializeField] private XRControls xrControls;
        [SerializeField] private MapTable mapTable;
        [SerializeField] private ShipAutopilot autopilot;

        [Header("Player")]
        [SerializeField] private XROrigin xrOrigin;
        [SerializeField] private Transform station;
        [SerializeField, Min(0f)] private float steeringHeightOffset = 0.7f;
        [SerializeField] private TrackedPoseDriver leftController;
        [SerializeField] private TrackedPoseDriver rightController;
        [SerializeField] private Transform leftVisual;
        [SerializeField] private Transform rightVisual;

        public enum Mode { General, Map, Steering, Autopiloting, HelmBraking }

        public Mode CurrentMode { get; private set; }
        public float WheelAngle => helm.WheelAngle;
        public float SteeringInput => helm.SteeringInput;
        public float SpeedPercent => throttle.SpeedPercent;

        private InputAction leftTrigger;
        private InputAction rightTrigger;
        private CurveInteractionCaster leftRay;
        private CurveInteractionCaster rightRay;
        private XRBaseInteractor[] handInteractors;

        private Mode DefaultMode => autopilot.IsAutopilot ? Mode.Autopiloting : Mode.General;

        void Awake()
        {
            var generalActions = xrControls.GeneralActions;
            leftTrigger = generalActions.FindAction("LeftTrigger", true);
            rightTrigger = generalActions.FindAction("RightTrigger", true);

            leftRay = (CurveInteractionCaster)leftController
                .GetComponentInChildren<NearFarInteractor>(true).farInteractionCaster;
            rightRay = (CurveInteractionCaster)rightController
                .GetComponentInChildren<NearFarInteractor>(true).farInteractionCaster;

            handInteractors =
                leftController.GetComponentsInChildren<XRBaseInteractor>(true)
                .Concat(rightController.GetComponentsInChildren<XRBaseInteractor>(true)).ToArray();
        }

        void OnDisable()
        {
            if (CurrentMode == Mode.Map) ExitMap();
            else ExitHelm();
        }

        void Update()
        {
            switch (CurrentMode)
            {
                case Mode.General:
                case Mode.Autopiloting:
                    CurrentMode = DefaultMode;
                    if (CanEnterMap()) EnterMap();
                    if (CanEnterHelm()) EnterHelm();
                    break;

                case Mode.Map:
                    if (mapTable.CloseRequested) ExitMap();
                    else mapTable.UpdateCamera();
                    break;

                case Mode.Steering:
                case Mode.HelmBraking:
                    if (helm.ExitRequested)
                    {
                        ExitHelm();
                        break;
                    }

                    if (CurrentMode == Mode.HelmBraking)
                    {
                        if (throttle.HasInput()) autopilot.TakeManualControl();
                        if (autopilot.IsAutopilot) break;
                        CurrentMode = Mode.Steering;
                    }

                    throttle.UpdateThrottle();
                    break;
            }
        }

        void LateUpdate()
        {
            if (CurrentMode is Mode.Steering or Mode.HelmBraking) 
                helm.UpdateWheel(leftController, rightController);
            else if (CurrentMode == Mode.Map) mapTable.UpdatePortLabels();
        }

        private bool CanEnterMap() => TriggerHits(mapTable.transform);

        private bool CanEnterHelm() => TriggerHits(transform)
            && !handInteractors.Any(interactor => interactor.hasSelection);

        // Press Trigger while pointing the target.
        private bool TriggerHits(Transform target) =>
            (leftTrigger.triggered && HitsTarget(leftRay, target))
            || (rightTrigger.triggered && HitsTarget(rightRay, target));

        // Check if the raycast hits the target.
        private static bool HitsTarget(CurveInteractionCaster caster, Transform target)
        {
            if (!caster.isActiveAndEnabled) return false;

            Transform origin = caster.effectiveCastOrigin;
            return Physics.Raycast(origin.position, origin.forward, out var hit, caster.castDistance,
                Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore)
                && hit.collider.transform.IsChildOf(target);
        }

        private void EnterMap()
        {
            if (!xrControls.SwitchActionMap(xrControls.MapActions)) return;

            CurrentMode = Mode.Map;
            mapTable.Open();
        }

        private void ExitMap()
        {
            CurrentMode = DefaultMode;
            mapTable.Close();
            xrControls.Restore();
        }

        private void EnterHelm()
        {
            if (!xrControls.SwitchActionMap(xrControls.HelmActions)) return;

            var head = xrOrigin.Camera.transform;

            // Teleport the player to the helm station and rotate them to face forward.
            xrOrigin.RotateAroundCameraUsingOriginUp(
                Mathf.DeltaAngle(head.eulerAngles.y, station.eulerAngles.y)
            );
            xrOrigin.Origin.transform.position +=
                Vector3.ProjectOnPlane(station.position - head.position, Vector3.up) +
                Vector3.up * steeringHeightOffset;

            helm.Begin(leftVisual, rightVisual);
            throttle.Begin();
            autopilot.CancelAutopilot();
            CurrentMode = autopilot.IsAutopilot ? Mode.HelmBraking : Mode.Steering;
            steeringHUD.SetActive(true);
        }

        private void ExitHelm()
        {
            if (CurrentMode is not (Mode.Steering or Mode.HelmBraking)) return;

            CurrentMode = DefaultMode;
            steeringHUD.SetActive(false);
            helm.End();
            xrOrigin.Origin.transform.position -= Vector3.up * steeringHeightOffset;

            xrControls.Restore();
        }
    }
}
