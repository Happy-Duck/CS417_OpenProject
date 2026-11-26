using System;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace _Sharyq
{
    public class FishNavigation : MonoBehaviour
    {
        private float Speed { get; set; }
        
        private float hungerLevel;
        public float Hunger
        {
            get => hungerLevel;
            set
            {
                if (value < 0f)
                {
                    hungerLevel = 0f;
                }
                else if (value > 1f)
                {
                    hungerLevel = 1f;
                }
                else 
                {
                    hungerLevel = value;
                }
            }
        }
        
        private float smellRange;
        public float Smell
        {
            get => smellRange;
            set
            {
                if (value < 0f)
                {
                    smellRange = 0f;
                }
                else if (value > 15f)
                {
                    smellRange = 15f;
                }
                else 
                {
                    smellRange = value;
                }
            }
        }
        public Hook HookObj { get; set; }

        private NavMeshAgent navMeshAgent;
        private float baitInterest;

        private void Awake()
        {
            navMeshAgent = GetComponent<NavMeshAgent>();
        }

        private void Start()
        {
            Vector3 pos = HookObj.transform.position;
            TryGetRandomNavMeshPosition(out pos);
            navMeshAgent.SetDestination(pos);
        }
        
        private bool TryGetRandomNavMeshPosition(out Vector3 position)
        {
            Vector3 center = transform.position;
            Vector3 randomPoint = center + Random.insideUnitSphere * smellRange;

            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, smellRange, NavMesh.AllAreas))
            {
                position = hit.position;
                return true;
            }

            position = center;
            return false;
        }

        private void Update()
        {
            if (Vector3.Distance(HookObj.transform.position, transform.position) < smellRange && !HookObj.Fish)
                baitInterest += Time.deltaTime * (1 + hungerLevel);
            else
            {
                baitInterest -= Time.deltaTime * hungerLevel;
            }

            if (baitInterest >= 1f)
            {
                navMeshAgent.SetDestination(HookObj.transform.position);
            }
        }
    }
}

