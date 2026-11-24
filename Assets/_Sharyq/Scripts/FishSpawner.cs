using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _Sharyq
{
    public class FishSpawner : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField]
        private GameObject[] fihhPrefabs;
        [SerializeField] 
        private float spawnRange = 10f;
        [SerializeField]
        private float timeBetweenSpawns = 10f;
        [SerializeField]
        private float timeVariance = 3f;
        [SerializeField]
        private int maxFihh = 5;

        [Header("Debug")] 
        [SerializeField] 
        private bool spawnEnabled = true;
        
        private int currentFihh = 0;
        private float _currentTime = 0f;
        private float _currentTimeMax = 0f;

        private void Start()
        {
            CreateMaxTime();
        }
        
        private void Update()
        {
            if (CanSpawnFihh())
                _currentTime += Time.deltaTime;

            if (_currentTime >= _currentTimeMax)
                SpawnFihh();
        }

        private bool CanSpawnFihh()
        {
            if (currentFihh >= maxFihh) return false;
            if (!spawnEnabled) return false;

            return true;
        }

        private void CreateMaxTime()
        {
            float rangeToAdd = Random.Range(0f, timeBetweenSpawns);
            _currentTimeMax += rangeToAdd;
        }

        private void SpawnFihh()
        {
            // Spawn Fish
            int randomIndex = Random.Range(0, fihhPrefabs.Length);
            Vector3 randomPosition = new Vector3(
                Random.Range(-spawnRange, spawnRange),
                0,
                Random.Range(-spawnRange, spawnRange)
            );
            
            Instantiate(fihhPrefabs[randomIndex], transform.position + randomPosition, Quaternion.identity);
            
            _currentTime = 0f;
            currentFihh++;
            CreateMaxTime();
        }
    }
}