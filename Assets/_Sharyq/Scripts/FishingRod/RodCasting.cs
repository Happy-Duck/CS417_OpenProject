using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace _Sharyq
{
    public class RodCasting : MonoBehaviour
    {
        [SerializeField]
        private GameObject hookObject;

        [Header("Rope")]
        [SerializeField]
        private RopeBuilder rope;
        [Tooltip("XR Origin root. Controller velocity is reported in tracking space, so it must be converted with this.")]
        [SerializeField]
        private Transform xrOrigin;

        [Header("Right hand Cast Controls")]
        [SerializeField]
        private InputActionReference castActionRightHandAsset;
        [SerializeField]
        InputActionProperty velocityRightAction;
        [Header("Left hand Cast Controls")]
        [SerializeField]
        private InputActionReference castActionLeftHandAsset;
        [SerializeField]
        InputActionProperty velocityLeftAction;
        
        [Header("Debug")]
        [SerializeField] private bool useDebugVelocity = true;
        [SerializeField] private float debugVelocity = 5f;
        [SerializeField] private Transform debugForwardSource;

        private XRGrabInteractable _grabInteractable;
        private NearFarInteractor _interactor;

        private InputActionReference _handToUse;
        private InputActionProperty _velocityToUse;

        private void Awake()
        {
            _grabInteractable = GetComponent<XRGrabInteractable>();
            _grabInteractable.selectEntered.AddListener(OnGrabbed);
            _grabInteractable.selectExited.AddListener(OnReleased);
            if (_grabInteractable == null)
                Debug.LogWarning("No XRGrabInteractable component found on " + gameObject.name);
            if (rope == null)
                Debug.LogWarning("No RopeBuilder assigned on " + gameObject.name);
            if (xrOrigin == null)
                Debug.LogWarning("No XR Origin assigned on " + gameObject.name + "; cast direction may be wrong.");
            hookObject.transform.SetParent(null, true);
        }

        private void OnEnable()
        {
            EnableCastInteraction();
        }

        private void OnDisable()
        {
            DisableCastInteraction();
        }

        public void OnGrabbed(SelectEnterEventArgs arg0)
        {
            _interactor = _grabInteractable.firstInteractorSelecting as NearFarInteractor;
            if (_interactor == null) return;
            switch (_interactor.handedness)
            {
                case InteractorHandedness.Left:
                    _handToUse = castActionLeftHandAsset;
                    _velocityToUse = velocityLeftAction;
                    break;
                case InteractorHandedness.Right:
                    _handToUse = castActionRightHandAsset;
                    _velocityToUse = velocityRightAction;
                    break;
                default:
                    _handToUse = null;
                    break;
            }

            EnableCastInteraction();
        }

        public void OnReleased(SelectExitEventArgs arg0)
        {
            DisableCastInteraction();
            _interactor = null;
            _handToUse = null;
        }

        // Hook this to a button / UnityEvent to reel the hook back in
        public void ReelIn()
        {
            if (rope != null) rope.ResetToHeld();
        }

        private void EnableCastInteraction()
        {
            if (_handToUse == null) return;
            _handToUse.action.started += StartCasting;
            _handToUse.action.canceled += FinishedCasting;
            _handToUse.action.Enable();
            _velocityToUse.action.Enable();
        }

        private void DisableCastInteraction()
        {
            if (_handToUse == null) return;
            _handToUse.action.started -= StartCasting;
            _handToUse.action.canceled -= FinishedCasting;
            _handToUse.action.Disable();
            _velocityToUse.action.Disable();
        }

        private void StartCasting(InputAction.CallbackContext obj)
        {
            // TODO: ADD JUICE TO WIND UP
            Debug.Log("Starting Casting");
        }

        private void FinishedCasting(InputAction.CallbackContext obj)
        {
            if (rope == null || !rope.IsHeld) return;

            Vector3 worldVel = GetWorldCastVelocity();

            Debug.Log($"Casted with Speed: {worldVel.magnitude:F2} u/s");
            rope.Cast(worldVel);
        }
        
        private Vector3 GetWorldCastVelocity()
        {
            if (useDebugVelocity)
                return GetDebugForward() * debugVelocity;

            if (_velocityToUse.action == null) return Vector3.zero;

            Vector3 trackingVel = _velocityToUse.action.ReadValue<Vector3>();
            Transform space = xrOrigin != null ? xrOrigin : transform;
            return space.TransformDirection(trackingVel);
        }

        private Vector3 GetDebugForward()
        {
            Transform source = debugForwardSource;
            if (source == null && Camera.main != null) source = Camera.main.transform;
            if (source == null) source = xrOrigin != null ? xrOrigin : transform;

            // Flatten so looking up/down doesn't change the cast heading
            Vector3 flat = Vector3.ProjectOnPlane(source.forward, Vector3.up);
            return flat.sqrMagnitude > 0.0001f ? flat.normalized : source.forward;
        }

        private void OnDestroy()
        {
            _grabInteractable.selectEntered.RemoveListener(OnGrabbed);
            _grabInteractable.selectExited.RemoveListener(OnReleased);
        }
    }
}