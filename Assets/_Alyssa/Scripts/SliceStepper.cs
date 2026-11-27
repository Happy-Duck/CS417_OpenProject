using UnityEngine;
using Shared;

namespace Alyssa
{
    public enum SliceAxis { X, Y, Z }

    [RequireComponent(typeof(Cuttable))]
    public class SliceStepper : MonoBehaviour
    {
        public GameObject slicePrefab;
        public Resource resource;
        public int slices = 4;
        public float length = 0.6f;
        public SliceAxis axis = SliceAxis.Z;
        public Vector3 sliceEuler;
        public Transform scaler;
        public Transform guide;

        [Tooltip("Tick for rolls: FinishedRoll supplies the slice prefab and count after spawning")]
        public bool waitForConfigure;

        Cuttable cuttable;
        BoxCollider box;
        Vector3 baseScale;
        Vector3 baseBoxSize;
        Vector3 baseBoxCenter;
        float step;
        int hitCount;
        bool ready;

        int Axis { get { return (int)axis; } }

        void Awake()
        {
            if (!waitForConfigure) Setup();
        }

        public void Configure(GameObject prefab, int count)
        {
            if (ready) return;
            if (prefab != null) slicePrefab = prefab;
            if (count > 0) slices = count;
            Setup();
        }

        void Setup()
        {
            if (ready) return;
            ready = true;

            slices = Mathf.Max(2, slices);
            step = length / slices;

            cuttable = GetComponent<Cuttable>();
            box = GetComponent<BoxCollider>();

            if (scaler != null) baseScale = scaler.localScale;
            if (box != null)
            {
                baseBoxSize = box.size;
                baseBoxCenter = box.center;
            }

            Cuttable.Result[] perHit = new Cuttable.Result[slices - 1];
            for (int i = 0; i < perHit.Length; i++) perHit[i] = MakeResult(i);

            cuttable.perHitResults = perHit;
            cuttable.finalResults = new Cuttable.Result[] { MakeResult(slices - 1) };
            cuttable.hitsRequired = slices - 1;
            cuttable.onHit.AddListener(Advance);

            MoveGuide();
        }

        Cuttable.Result MakeResult(int index)
        {
            Vector3 offset = Vector3.zero;
            offset[Axis] = length * 0.5f - (index + 0.5f) * step;

            Cuttable.Result r = new Cuttable.Result();
            r.prefab = slicePrefab;
            r.localOffset = offset;
            r.localEuler = sliceEuler;
            r.resource = resource;
            r.resourceAmount = 1f;
            return r;
        }

        void Advance()
        {
            hitCount++;
            float removed = hitCount * step;
            float factor = Mathf.Max((length - removed) / length, 0.001f);

            if (scaler != null)
            {
                Vector3 scale = baseScale;
                scale[Axis] = baseScale[Axis] * factor;
                scaler.localScale = scale;

                Vector3 pos = Vector3.zero;
                pos[Axis] = -removed * 0.5f;
                scaler.localPosition = pos;
            }

            if (box != null)
            {
                Vector3 size = baseBoxSize;
                size[Axis] = baseBoxSize[Axis] * factor;
                box.size = size;

                Vector3 center = baseBoxCenter;
                center[Axis] = baseBoxCenter[Axis] * factor - removed * 0.5f;
                box.center = center;
            }

            MoveGuide();
        }

        void MoveGuide()
        {
            if (guide == null) return;

            Vector3 pos = guide.localPosition;
            pos[Axis] = length * 0.5f - (hitCount + 1) * step;
            guide.localPosition = pos;
        }
    }
}