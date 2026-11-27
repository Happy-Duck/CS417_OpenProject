using System.Collections;
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
        public GameObject handlePrefab;
        [Tooltip("Empty child at the FINAL pose of the rolled roll (far edge, resting on the mat)")]
        public Transform previewAnchor;
        [Tooltip("Used only if a recipe has no Rolled Prefab assigned")]
        public GameObject fallbackRolledPrefab;

        public Vector3 rollDirection = Vector3.right;
        public float length = 0.6f;
        public Axis previewLengthAxis = Axis.Z;
        public int spins = 2;
        public float handleHeight = 0.03f;
        [Range(0.5f, 1f)] public float finishAt = 0.95f;

        public UnityEvent onRollStart;
        public UnityEvent onRolled;

        bool started, finished;
        float progress;
        Vector3 dir;
        Transform flatRoot;

        GameObject rolledPrefab;
        Transform rolledPreview;
        Vector3 previewFinalPos, previewBaseScale;
        Quaternion previewBaseRot;

        GameObject handle;
        Component handleGrab;
        PropertyInfo handleSelected;
        Vector3 handleRestLocal;
        Quaternion handleLocalRot;

        void Awake()
        {
            if (builder == null) builder = GetComponent<RollBuilder>();
            dir = rollDirection.normalized;
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

            foreach (MonoBehaviour m in GetComponents<MonoBehaviour>())
                if (m != null && m.GetType().Name == "XRGrabInteractable") m.enabled = false;

            // Pivot at the far edge so scaling squashes everything toward it
            flatRoot = new GameObject("FlatRoot").transform;
            flatRoot.SetParent(transform, false);
            flatRoot.localPosition = dir * (length * 0.5f);

            List<Transform> kids = new List<Transform>();
            foreach (Transform c in transform)
                if (c != flatRoot && c != previewAnchor) kids.Add(c);
            foreach (Transform c in kids) c.SetParent(flatRoot, true);

            // Pick the prefab for the recipe that was just completed
            RollData recipe = builder != null ? builder.Roll : null;
            rolledPrefab = (recipe != null && recipe.rolledPrefab != null) ? recipe.rolledPrefab : fallbackRolledPrefab;

            SpawnPreview();
            SpawnHandle();
            onRollStart.Invoke();
        }

        void SpawnPreview()
        {
            if (rolledPrefab == null || previewAnchor == null) return;

            GameObject g = Instantiate(rolledPrefab, previewAnchor.position, previewAnchor.rotation, transform);
            rolledPreview = g.transform;

            previewFinalPos = rolledPreview.localPosition;
            previewBaseRot = rolledPreview.localRotation;
            previewBaseScale = rolledPreview.localScale;

            foreach (Collider c in g.GetComponentsInChildren<Collider>()) c.enabled = false;
            Rigidbody rb = g.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = true;

            // Scripts that other components depend on go a frame later
            Cuttable cut = g.GetComponent<Cuttable>();
            foreach (MonoBehaviour m in g.GetComponentsInChildren<MonoBehaviour>())
                if (m != null && m != cut) Destroy(m);

            StartCoroutine(DestroyNextFrame(g, cut, rb));
            g.SetActive(false);
        }

        IEnumerator DestroyNextFrame(GameObject g, Cuttable cut, Rigidbody rb)
        {
            yield return null;
            if (cut != null) Destroy(cut);
            if (rb != null) Destroy(rb);
            if (g != null)
                foreach (LineRenderer lr in g.GetComponentsInChildren<LineRenderer>(true))
                    Destroy(lr.gameObject);
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

                progress = Mathf.Max(progress, Mathf.Clamp01(p));
                if (progress >= finishAt) Finish();
            }
            else
            {
                handle.transform.SetPositionAndRotation(
                    transform.TransformPoint(handleRestLocal + dir * (progress * length)),
                    transform.rotation * handleLocalRot);
            }
        }

        void Apply()
        {
            float p = Mathf.Clamp(progress, 0.001f, 0.999f);

            Vector3 axisMask = new Vector3(Mathf.Abs(dir.x), Mathf.Abs(dir.y), Mathf.Abs(dir.z));
            flatRoot.localScale = Vector3.one - axisMask * p;

            if (rolledPreview == null) return;

            rolledPreview.gameObject.SetActive(progress > 0.01f);
            rolledPreview.localPosition = previewFinalPos - dir * length * (1f - p);

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

            if (rolledPrefab != null && previewAnchor != null)
            {
                GameObject done = Instantiate(rolledPrefab, previewAnchor.position, previewAnchor.rotation);
                FinishedRoll fr = done.GetComponent<FinishedRoll>();
                if (fr != null) fr.Init(builder != null ? builder.Roll : null);
            }

            if (handle != null) Destroy(handle);
            Destroy(gameObject);
        }
    }
}