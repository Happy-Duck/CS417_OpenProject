using System.Collections;
using Shared;
using UnityEngine;

namespace Dylan
{
    public class SeagullSpawner : MonoBehaviour
    {
        [SerializeField] private CookingEvents cookingEvents;
        [SerializeField] private SeagullAI seagullPrefab;
        [SerializeField] private Transform[] spawnPoints;
        [SerializeField, Range(0f, 1f)] private float spawnChance = 0.5f;
        [SerializeField, Min(0f)] private float spawnDelay = 3f;

        [Header("Dish Test")]
        [SerializeField] private GameObject testDishPrefab;
        [SerializeField] private Transform testDishSpawnPoint;

        private void OnEnable() => cookingEvents.DishCompleted.AddListener(TrySpawn);

        private void OnDisable() => cookingEvents.DishCompleted.RemoveListener(TrySpawn);

        public void SimulateDishComplete()
        {
            Instantiate(
                testDishPrefab, testDishSpawnPoint.position, testDishSpawnPoint.rotation, transform.parent);
            cookingEvents.DishCompleted.Invoke();
        }

        public void TrySpawn()
        {
            if (Random.value >= spawnChance) return;

            var scene = gameObject.scene;
            GameObject[] dishes = System.Array.FindAll(
                GameObject.FindGameObjectsWithTag("Dish"), dish => dish.scene == scene);
            if (dishes.Length == 0) return;

            GameObject target = dishes[Random.Range(0, dishes.Length)];
            if (!target.TryGetComponent(out ResourceBinding binding)) return;

            StartCoroutine(SpawnAfterDelay(target.transform, binding.Asset));
        }

        private IEnumerator SpawnAfterDelay(Transform dish, Resource resource)
        {
            for (float remaining = spawnDelay; remaining > 0f; remaining -= Time.deltaTime)
            {
                yield return null;
                if (dish == null) yield break;
            }

            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            SeagullAI seagull = Instantiate(seagullPrefab, spawnPoint.position, spawnPoint.rotation, transform.parent);
            seagull.target = dish;
            seagull.resource = resource;
        }
    }
}
