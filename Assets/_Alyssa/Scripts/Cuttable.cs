using UnityEngine;
using UnityEngine.Events;
using Shared;

namespace Alyssa
{
    public class Cuttable : MonoBehaviour
    {
        [System.Serializable]
        public class Result
        {
            public GameObject prefab;
            public Transform spawnAt;
            public Vector3 localOffset;
            public Vector3 localEuler;
            public string resourceName;
            public int resourceAmount = 1;
        }

        [Header("Cutting")]
        public int hitsRequired = 1;
        public float minKnifeSpeed = 1f;
        public float hitCooldown = 0.35f;
        public bool requiresBoard = true;
        public bool requireZone;
        public GameObject[] blockers;
        public float unblockDelay = 1f;
        public KnifeType requiredTool;

        [Header("Results")]
        public Result[] perHitResults;
        public Result[] finalResults;
        public GameObject[] hideOnHit;
        public float popForce = 0.5f;

        [Header("Events")]
        public UnityEvent onHit;
        public UnityEvent onFinalCut;
        public UnityEvent onTooSlow;
        public UnityEvent onNotOnBoard;
        public UnityEvent onWrongSpot;
        public UnityEvent onBlocked;
        public UnityEvent onWrongTool;

        [HideInInspector] public int boardContacts;

        int hits;
        float lastHitTime = -10f;
        float lastZoneTime = -10f;
        float lastBlockedTime = -10f;
        float lastWrongToolTime = -10f;
        float unblockedTime = -10f;
        bool wasBlocked;
        bool wrongSpotPending;

        public void TryHit(float knifeSpeed, KnifeType tool, bool viaZone = false)
        {
            if (viaZone) lastZoneTime = Time.time;

            if (tool != requiredTool)
            {
                if (knifeSpeed >= minKnifeSpeed && Time.time - lastWrongToolTime > hitCooldown)
                {
                    lastWrongToolTime = Time.time;
                    onWrongTool.Invoke();
                }
                return;
            }

            if (requireZone && !viaZone)
            {
                if (knifeSpeed >= minKnifeSpeed) wrongSpotPending = true;
                return;
            }

            if (IsBlocked())
            {
                if (knifeSpeed >= minKnifeSpeed && Time.time - lastBlockedTime > hitCooldown)
                {
                    lastBlockedTime = Time.time;
                    onBlocked.Invoke();
                }
                return;
            }

            if (Time.time - unblockedTime < unblockDelay) return;

            if (requiresBoard && boardContacts <= 0) { onNotOnBoard.Invoke(); return; }
            if (Time.time - lastHitTime < hitCooldown) return;
            if (knifeSpeed < minKnifeSpeed) { onTooSlow.Invoke(); return; }

            lastHitTime = Time.time;

            if (perHitResults != null && hits < perHitResults.Length) Spawn(perHitResults[hits]);
            if (hideOnHit != null && hits < hideOnHit.Length && hideOnHit[hits] != null)
                hideOnHit[hits].SetActive(false);

            hits++;
            onHit.Invoke();

            if (hits >= hitsRequired) FinalCut();
        }

        bool IsBlocked()
        {
            if (blockers == null) return false;
            foreach (GameObject b in blockers)
                if (b != null) return true;
            return false;
        }

        void Start()
        {
            wasBlocked = IsBlocked();
        }

        void Update()
        {
            bool blocked = IsBlocked();
            if (wasBlocked && !blocked) unblockedTime = Time.time;
            wasBlocked = blocked;

            if (!wrongSpotPending) return;

            wrongSpotPending = false;
            // A zone hit in the same swing means the body touch was just the blade passing through
            if (Time.time - lastZoneTime > 0.1f) onWrongSpot.Invoke();
        }

        void FinalCut()
        {
            onFinalCut.Invoke();
            if (finalResults != null)
                foreach (Result r in finalResults) Spawn(r);
            Destroy(gameObject);
        }

        void Spawn(Result r)
        {
            if (r.prefab != null)
            {
                Vector3 pos = transform.TransformPoint(r.localOffset);
                Quaternion rot = transform.rotation;
                if (r.spawnAt != null)
                {
                    Renderer rend = r.spawnAt.GetComponentInChildren<Renderer>();
                    pos = rend != null ? rend.bounds.center : r.spawnAt.position;
                    rot = r.spawnAt.rotation;
                }
                rot *= Quaternion.Euler(r.localEuler);
                GameObject go = Instantiate(r.prefab, pos, rot, SceneRoot.Root);

                // New pieces inherit freshness and start decaying once cut
                Freshness parentFresh = GetComponentInParent<Freshness>();
                Freshness newFresh = go.GetComponent<Freshness>();
                if (newFresh != null)
                {
                    if (parentFresh != null) newFresh.value = parentFresh.value;
                    newFresh.isDecaying = true;
                }

                Rigidbody rb = go.GetComponent<Rigidbody>();
                if (rb != null)
                    rb.AddForce((pos - transform.position).normalized * popForce, ForceMode.Impulse);
            }

            if (!string.IsNullOrEmpty(r.resourceName) && ResourceManager.Instance != null)
                ResourceManager.Instance.Add(r.resourceName, r.resourceAmount);
        }
    }
}