using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;

namespace Alyssa
{
    public class NoriRoller : MonoBehaviour
    {
        public enum Axis { X, Y, Z }

        public RollBuilder builder;
        public GameObject handlePrefab;     // grabbable bar spawned at the near edge
        public Transform rolledPreview;     // rolled mesh, child of the nori, placed at the FINAL pose
        public GameObject rolledPrefab;     // full grabbable roll that replaces the nori at the end

        [Tooltip("Axis-aligned, in nori local space. Use (-1,0,0) if it rolls the wrong way.")]
        public Vector3 rollDirection = Vector3.right;
        [Tooltip("Flat nori size along the roll direction, in nori local units")]
        public float length = 0.6f;
        [Tooltip("The long axis of the rolled mesh, in the preview's local space")]
        public Axis previewLengthAxis = Axis.Z;
        public int spins = 2;               // whole turns; negative reverses the spin
        public float handleHeight = 0.03f;
        [Range(0.5f, 1f)] public float finishAt = 0.95f;

        public UnityEvent onRollStart;
        public UnityEvent onRolled;

        bool started, finished;
        float progress;
        Vector3 dir;
        Transform flatRoot;

        GameObject handle;
        Component handleGrab;
        PropertyInfo handleSelected;
        Vector3 handleRestLocal;
        Quaternion handleLocalRot;

        Vector3 previewFinalPos, previewBaseScale;
        Quaternion previewBaseRot;

        void Awake()
        {
            if (builder == null) builder = GetComponent<RollBuilder>();
            dir = rollDirection.normalized;

            if (rolledPreview != null)
            {
                previewFinalPos = rolledPreview.localPosition;
                previewBaseRot = rolledPreview.localRotation;
                previewBaseScale = rolledPreview.localScale;
                rolledPreview.gameObject.SetActive(false);
            }
        }

        void Update()
        {
            if (finished) return;

            if (!started)
            {
                if (builder != null && builder.IsComplete) Begin();
                return;
            }

            UpdateHandle();
            Apply();
        }

        void Begin()
        {
            started = true;

            // Nori can't be picked up while it's being rolled
            foreach (MonoBehaviour m in GetComponents<MonoBehaviour>())
                if (m != null && m.GetType().Name == "XRGrabInteractable") m.enabled = false;

            // Pivot at the far edge so scaling squashes everything toward it
            flatRoot = new GameObject("FlatRoot").transform;
            flatRoot.SetParent(transform, false);
            flatRoot.localPosition = dir * (length * 0.5f);

            List<Transform> kids = new List<Transform>();
            foreach (Transform c in transform)
                if (c != rolledPreview && c != flatRoot) kids.Add(c);
            foreach (Transform c in kids) c.SetParent(flatRoot, true);

            SpawnHandle();
            onRollStart.Invoke();
        }

        void SpawnHandle()
        {
            if (handlePrefab == null) return;

            handleRestLocal = -dir * (length * 0.5f) + Vector3.up * handleHeight;
            handle = Instantiate(handlePrefab,
                transform.TransformPoint(handleRestLocal),
                transform.rotation * handlePrefab.transform.rotation);
            handleLocalRot = Quaternion.Inverse(transform.rotation) * handle.transform.rotation;

            foreach (MonoBehaviour m in handle.GetComponents<MonoBehaviour>())
            {
                if (m != null && m.GetType().Name == "XRGrabInteractable")
                {
                    handleGrab = m;
                    handleSelected = m.GetType().GetProperty("isSelected");
                }
            }
        }

        bool HandleHeld()
        {
            return handleGrab != null && handleSelected != null && (bool)handleSelected.GetValue(handleGrab);
        }

        void UpdateHandle()
        {
            if (handle == null) return;

            if (HandleHeld())
            {
                Vector3 local = transform.InverseTransformPoint(handle.transform.position);
                float p = (Vector3.Dot(local, dir) + length * 0.5f) / length;

                // Progress only moves forward, so it can't be unrolled
                progress = Mathf.Max(progress, Mathf.Clamp01(p));
                if (progress >= finishAt) Finish();
            }
            else
            {
                // Released early: park the handle at the current roll position
                handle.transform.SetPositionAndRotation(
                    transform.TransformPoint(handleRestLocal + dir * (progress * length)),
                    transform.rotation * handleLocalRot);
            }
        }

        void Apply()
        {
            float p = Mathf.Clamp(progress, 0.001f, 0.999f);

            // Flat part shrinks toward the far edge
            Vector3 axisMask = new Vector3(Mathf.Abs(dir.x), Mathf.Abs(dir.y), Mathf.Abs(dir.z));
            flatRoot.localScale = Vector3.one - axisMask * p;

            if (rolledPreview == null) return;

            rolledPreview.gameObject.SetActive(progress > 0.01f);
            rolledPreview.localPosition = previewFinalPos - dir * length * (1f - p);

            // Grows across its cross-section, keeps full length
            Vector3 grow = new Vector3(p, p, p);
            grow[(int)previewLengthAxis] = 1f;
            rolledPreview.localScale = Vector3.Scale(previewBaseScale, grow);

            Vector3 spinAxis = Vector3.zero;
            spinAxis[(int)previewLengthAxis] = 1f;
            rolledPreview.localRotation = previewBaseRot * Quaternion.AngleAxis(p * 360f * spins, spinAxis);
        }

        void Finish()
        {
            finished = true;
            onRolled.Invoke();

            if (rolledPrefab != null && rolledPreview != null)
            {
                Transform parent = rolledPreview.parent;
                Vector3 pos = parent.TransformPoint(previewFinalPos);
                Quaternion rot = parent.rotation * previewBaseRot;

                GameObject done = Instantiate(rolledPrefab, pos, rot);
                FinishedRoll fr = done.GetComponent<FinishedRoll>();
                if (fr != null) fr.roll = builder != null ? builder.Roll : null;
            }

            if (handle != null) Destroy(handle);
            Destroy(gameObject);
        }
    }
}