using UnityEngine;
using UnityEngine.Events;
using Shared;

namespace Alyssa
{
    public class RiceBowl : MonoBehaviour
    {
        public Resource rice;
        public float costPerScoop = 1f;
        public float emptyCooldown = 1f;
        public UnityEvent onScoop;
        public UnityEvent onEmpty;

        float lastEmptyTime = -10f;

        public bool TryScoop()
        {
            if (rice != null && !rice.TrySpend(costPerScoop))
            {
                if (Time.time - lastEmptyTime >= emptyCooldown)
                {
                    lastEmptyTime = Time.time;
                    onEmpty.Invoke();
                }
                return false;
            }

            onScoop.Invoke();
            return true;
        }
    }
}