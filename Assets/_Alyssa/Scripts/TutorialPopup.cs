using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace Alyssa
{
    public class TutorialPopup : MonoBehaviour
    {
        public enum Trigger { Proximity, OnGrab }

        public string title = "";
        [TextArea] public string message = "Do the thing.";
        [Tooltip("Optional. If filled, these pages are used instead of Message (needs Require Confirm).")]
        [TextArea] public string[] pages;
        public string id = "";                         // unique per popup; defaults to the object name
        public Trigger triggerMode = Trigger.Proximity;
        public float showDistance = 1.5f;              // Proximity only
        [Tooltip("Player must press Next / Got it. Off = auto-hides after Hide After Seconds.")]
        public bool requireConfirm = true;
        public float hideAfterSeconds = 6f;            // only when Require Confirm is off
        public Vector3 offset = new Vector3(0, 0.4f, 0);   // world meters above the object
        public float characterSize = 0.008f;           // lower this if panels look too big
        public bool onlyWhenPlaced = false;            // for grid items like the soil bed
        public TutorialPanel.PanelStyle style = new TutorialPanel.PanelStyle();
        public UnityEvent onConfirmed;                 // hook a sound or the next tutorial step here

        static readonly HashSet<string> seen = new();  // each popup only completes once per Play

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() { seen.Clear(); }

        TutorialPanel panel;
        //Shared.GridOccupant occ;
        Camera cam;

        void Start()
        {
            if (string.IsNullOrEmpty(id)) id = name;
            //occ = GetComponent<Shared.GridOccupant>();
            if (triggerMode == Trigger.OnGrab)
            {
                var grab = GetComponent<XRGrabInteractable>();
                if (grab != null) grab.selectEntered.AddListener(args =>
                {
                    // Sockets (like the knife block) also "select" the item; only a hand or controller should trigger it
                    if (args.interactorObject is XRSocketInteractor) return;
                    Show();
                });
                else Debug.LogWarning("TutorialPopup (OnGrab) needs an XRGrabInteractable on " + name, this);
            }
        }

        Camera Cam()
        {
            if (cam == null) cam = Camera.main;
            if (cam == null) cam = FindAnyObjectByType<Camera>();
            return cam;
        }

        void Update()
        {
            var c = Cam();
            if (c == null) return;

            float dist = Vector3.Distance(c.transform.position, transform.position);

            if (panel == null)
            {
                if (triggerMode != Trigger.Proximity || seen.Contains(id)) return;
                //if (onlyWhenPlaced && occ != null && !occ.IsPlaced) return;
                if (dist < showDistance) Show();
                return;
            }

            // Walked away: close it. An unconfirmed popup comes back when the player returns.
            if (triggerMode == Trigger.Proximity && dist > showDistance + 1.5f) panel.Dismiss();
        }

        void Show()
        {
            if (panel != null || seen.Contains(id)) return;
            if (requireConfirm && TutorialPanel.ModalOpen) return;   // wait for the open one

            string[] content = (pages != null && pages.Length > 0) ? pages : new[] { message };

            if (!requireConfirm) seen.Add(id);
            panel = TutorialPanel.Create(title, content, transform, offset, requireConfirm,
                requireConfirm ? 0f : hideAfterSeconds, characterSize, style, Closed);
        }

        void Closed(bool confirmed)
        {
            panel = null;
            if (!confirmed) return;

            seen.Add(id);
            onConfirmed.Invoke();
        }

        void OnDestroy()
        {
            if (panel != null) panel.Dismiss(true);
        }
    }
}