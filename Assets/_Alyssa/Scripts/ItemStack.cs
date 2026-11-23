using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Shared;

namespace Alyssa
{
    public class ItemStack : MonoBehaviour
    {
        public GameObject prefab;
        public Transform spawnPoint;
        public Resource cost;
        public float costPerItem = 1f;
        public float takeDistance = 0.12f;
        public float respawnDelay = 0.5f;
        public float popInTime = 0.2f;
        public UnityEvent onResupply;
        public UnityEvent onEmpty;

        readonly List<GameObject> spawned = new List<GameObject>();
        GameObject current;
        float emptySince = -1f;
        bool announcedEmpty;

        void Start()
        {
            TrySpawn(false);
        }

        void Update()
        {
            if (current != null && Vector3.Distance(current.transform.position, spawnPoint.position) < takeDistance)
            {
                emptySince = -1f;
                return;
            }

            // An item put back on the stack becomes the top item again
            GameObject nearby = FindNearby();
            if (nearby != null)
            {
                current = nearby;
                emptySince = -1f;
                return;
            }

            if (emptySince < 0f) emptySince = Time.time;
            if (Time.time - emptySince >= respawnDelay) TrySpawn(true);
        }

        GameObject FindNearby()
        {
            spawned.RemoveAll(g => g == null);
            foreach (GameObject g in spawned)
                if (Vector3.Distance(g.transform.position, spawnPoint.position) < takeDistance) return g;
            return null;
        }

        void TrySpawn(bool announce)
        {
            if (cost != null && !cost.TrySpend(costPerItem))
            {
                if (!announcedEmpty)
                {
                    announcedEmpty = true;
                    onEmpty.Invoke();
                }
                return;
            }

            announcedEmpty = false;
            current = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation, SceneRoot.Root);
            spawned.Add(current);
            emptySince = -1f;

            StartCoroutine(PopIn(current.transform, prefab.transform.localScale));
            if (announce) onResupply.Invoke();
        }

        IEnumerator PopIn(Transform item, Vector3 targetScale)
        {
            float t = 0f;
            while (item != null && t < popInTime)
            {
                t += Time.deltaTime;
                float k = Mathf.SmoothStep(0f, 1f, t / popInTime);
                item.localScale = targetScale * k;
                yield return null;
            }

            if (item != null) item.localScale = targetScale;
        }
    }
}