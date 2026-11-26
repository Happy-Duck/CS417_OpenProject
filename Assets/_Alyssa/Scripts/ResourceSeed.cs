using UnityEngine;
using Shared;

namespace Alyssa
{
    [DefaultExecutionOrder(-50)]
    public class ResourceSeed : MonoBehaviour
    {
        [System.Serializable]
        public class Entry
        {
            public Resource resource;
            public float amount;
        }

        public Entry[] entries;

        void Awake()
        {
            foreach (Entry e in entries)
                if (e.resource != null && e.resource.Value < e.amount)
                    e.resource.Add(e.amount - e.resource.Value);
        }
    }
}