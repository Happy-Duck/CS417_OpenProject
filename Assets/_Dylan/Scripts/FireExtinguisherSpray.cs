using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace Dylan
{
    [RequireComponent(typeof(XRGrabInteractable))]
    public class FireExtinguisherSpray : MonoBehaviour
    {
        [SerializeField] private InputActionReference leftActivate;
        [SerializeField] private InputActionReference rightActivate;
        [SerializeField] private ParticleSystem pressureJet;
        [SerializeField] private ParticleSystem powderCloud;
        [SerializeField] private RectTransform holdPrompt;

        private XRGrabInteractable grab;
        private Camera viewCamera;
        private Vector3 promptPosition;
        private Quaternion promptRotation;
        private bool heldByHand;

        void Awake()
        {
            grab = GetComponent<XRGrabInteractable>();
            promptPosition = holdPrompt.localPosition;
            promptRotation = holdPrompt.localRotation;
        }

        void OnEnable()
        {
            Application.onBeforeRender += FollowCamera;
            grab.selectEntered.AddListener(OnSelectionChanged);
            grab.selectExited.AddListener(OnSelectionChanged);
            RefreshHeldState();
        }

        void OnDisable()
        {
            Application.onBeforeRender -= FollowCamera;
            grab.selectEntered.RemoveListener(OnSelectionChanged);
            grab.selectExited.RemoveListener(OnSelectionChanged);
            SetHeldByHand(false);
        }

        void Update()
        {
            Spray(heldByHand &&
                (leftActivate.action.IsPressed() || rightActivate.action.IsPressed()));
        }

        void LateUpdate() => FollowCamera();

        [BeforeRenderOrder(10000)]
        private void FollowCamera()
        {
            if (viewCamera == null) return;

            var view = viewCamera.transform;
            holdPrompt.SetPositionAndRotation(
                view.TransformPoint(promptPosition), view.rotation * promptRotation
            );
        }

        private void OnSelectionChanged(BaseInteractionEventArgs args) => RefreshHeldState();

        private void RefreshHeldState()
        {
            foreach (var interactor in grab.interactorsSelecting)
            {
                if (interactor is XRBaseInputInteractor hand && hand.handedness != InteractorHandedness.None)
                {
                    SetHeldByHand(true);
                    return;
                }
            }
            SetHeldByHand(false);
        }

        private void SetHeldByHand(bool held)
        {
            heldByHand = held;
            viewCamera = held ? Camera.main : null;
            FollowCamera();
            holdPrompt.gameObject.SetActive(viewCamera != null);
            if (!held) Spray(false);
        }

        private void Spray(bool active)
        {
            if (active)
            {
                pressureJet.Play(false);
                powderCloud.Play(false);
            } else {
                pressureJet.Stop(false, ParticleSystemStopBehavior.StopEmitting);
                powderCloud.Stop(false, ParticleSystemStopBehavior.StopEmitting);
            }
        }
    }
}

