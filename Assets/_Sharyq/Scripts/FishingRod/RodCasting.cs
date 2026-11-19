using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace _Sharyq
{
    public class RodCasting : MonoBehaviour
    {
        [SerializeField]
        private GameObject hookObject;
        
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
        
        private XRGrabInteractable _grabInteractable;
        private NearFarInteractor _interactor;
        
        private InputActionReference _handToUse;
        private InputActionProperty _velocityToUse;

        private void Awake()
        {
            _grabInteractable = GetComponent<XRGrabInteractable>();
            if (_grabInteractable == null)
                Debug.LogWarning("No XRGrabInteractable component found on " + gameObject.name);
        }

        private void OnEnable()
        {
            EnableCastInteraction();
        }

        private void OnDisable()
        {
            DisableCastInteraction();
        }

        public void OnGrabbed()
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
        
        public void OnReleased()
        {
            DisableCastInteraction();
            _interactor = null;
            _handToUse = null;
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
            Debug.Log($"Starting Casting");
        }
        
        private void FinishedCasting(InputAction.CallbackContext obj)
        {
            Vector3 trackingVel = _velocityToUse.action.ReadValue<Vector3>();
            Vector3 worldVel = transform.TransformDirection(trackingVel);
            Debug.Log($"Casted with Speed: {worldVel.magnitude:F2} u/s");
        }

    }
}
