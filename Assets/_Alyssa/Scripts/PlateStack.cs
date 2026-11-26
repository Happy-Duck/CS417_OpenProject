using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Alyssa
{
    public class PlateStack : MonoBehaviour
    {
        public GameObject platePrefab;
        public Transform spawnPoint;
        public float takeDistance = 0.12f;
        public float respawnDelay = 0.5f;
        public float popInTime = 0.2f;
        public UnityEvent onResupply;

        GameObject current;
        float emptySince = -1f;

        void Start()
        {
            Spawn(false);
        }

        void Update()
        {
            if (current != null && Vector3.Distance(current.transform.position, spawnPoint.position) < takeDistance)
            {
                emptySince = -1f;
                return;
            }

            // A plate put back on the stack becomes the top plate again
            Plate nearby = FindNearbyPlate();
            if (nearby != null)
            {
                current = nearby.gameObject;
                emptySince = -1f;
                return;
            }

            if (emptySince < 0f) emptySince = Time.time;
            if (Time.time - emptySince >= respawnDelay) Spawn(true);
        }

        Plate FindNearbyPlate()
        {
            foreach (Collider c in Physics.OverlapSphere(spawnPoint.position, takeDistance))
            {
                Plate p = c.GetComponentInParent<Plate>();
                if (p != null) return p;
            }
            return null;
        }

        void Spawn(bool announce)
        {
            current = Instantiate(platePrefab, spawnPoint.position, spawnPoint.rotation, SceneRoot.Root);
            emptySince = -1f;

            StartCoroutine(PopIn(current.transform, platePrefab.transform.localScale));
            if (announce) onResupply.Invoke();
        }

        IEnumerator PopIn(Transform plate, Vector3 targetScale)
        {
            float t = 0f;
            while (plate != null && t < popInTime)
            {
                t += Time.deltaTime;
                float k = Mathf.SmoothStep(0f, 1f, t / popInTime);
                plate.localScale = targetScale * k;
                yield return null;
            }

            if (plate != null) plate.localScale = targetScale;
        }
    }
}