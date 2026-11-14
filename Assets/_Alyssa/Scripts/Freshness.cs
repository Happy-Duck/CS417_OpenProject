using UnityEngine;
using UnityEngine.Events;

namespace Alyssa
{
    public class Freshness : MonoBehaviour
    {
        public float value = 100f;
        public float decayPerSecond = 0.5f;
        public bool isDecaying = false;
        public GameObject spoiledPrefab;
        public UnityEvent onSpoiled;

        void Update()
        {
            if (!isDecaying) return;

            value -= decayPerSecond * Time.deltaTime;
            if (value <= 0f)
            {
                onSpoiled.Invoke();
                if (spoiledPrefab != null)
                    Instantiate(spoiledPrefab, transform.position, transform.rotation, SceneRoot.Root);
                Destroy(gameObject);
            }
        }
    }
}