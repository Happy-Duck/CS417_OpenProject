using System.Linq;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Locomotion;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

namespace Dylan
{
    public sealed class XRControls : MonoBehaviour
    {
        [SerializeField] private XROrigin xrOrigin;
        [SerializeField] private PlayerInput playerInput;

        private bool isSuspended;
        private GameObject[] interactorObjects;
        private Behaviour[] behaviours;
        private bool[] interactorWasActive;
        private bool[] behaviourWasEnabled;

        internal InputActionMap GeneralActions => playerInput.actions.FindActionMap("General", true);
        internal InputActionMap HelmActions => playerInput.actions.FindActionMap("Helm", true);
        internal InputActionMap MapActions => playerInput.actions.FindActionMap("Map", true);
        internal bool IsSuspended => isSuspended;

        void Awake()
        {
            interactorObjects = xrOrigin.GetComponentsInChildren<XRBaseInteractor>(true)
                .Select(interactor => interactor.gameObject).Distinct().ToArray();
            behaviours = xrOrigin.GetComponentsInChildren<ControllerInputActionManager>(true)
                .Cast<Behaviour>()
                .Concat(xrOrigin.GetComponentsInChildren<LocomotionProvider>(true)).ToArray();
            interactorWasActive = new bool[interactorObjects.Length];
            behaviourWasEnabled = new bool[behaviours.Length];
        }

        void OnEnable() => playerInput.ActivateInput();

        void OnDisable()
        {
            playerInput.DeactivateInput();
            playerInput.currentActionMap = null;
            Restore();
        }

        internal bool SwitchActionMap(InputActionMap modeActions)
        {
            if (isSuspended || modeActions.asset != playerInput.actions)
                return false;

            isSuspended = true;
            SuspendGeneralActions();
            playerInput.SwitchCurrentActionMap(modeActions.name);
            return true;
        }

        internal void Restore()
        {
            if (!isSuspended) return;
            isSuspended = false;
            playerInput.SwitchCurrentActionMap(GeneralActions.name);

            for (int i = 0; i < behaviours.Length; i++)
                if (behaviours[i] != null)
                    behaviours[i].enabled = behaviourWasEnabled[i];
            for (int i = 0; i < interactorObjects.Length; i++)
                interactorObjects[i]?.SetActive(interactorWasActive[i]);
        }

        private void SuspendGeneralActions()
        {
            for (int i = 0; i < interactorObjects.Length; i++)
                interactorWasActive[i] = interactorObjects[i].activeSelf;
            for (int i = 0; i < behaviours.Length; i++)
                behaviourWasEnabled[i] = behaviours[i].enabled;

            foreach (var behaviour in behaviours)
                behaviour.enabled = false;
            foreach (var interactor in interactorObjects)
                interactor.SetActive(false);
        }
    }
}
