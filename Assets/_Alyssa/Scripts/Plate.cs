using UnityEngine;
using UnityEngine.Events;

namespace Alyssa
{
    public class Plate : MonoBehaviour
    {
        public BoxCollider zone;
        public Transform[] slots;
        public string acceptedItem;
        public string dishName = "Tuna Sashimi Plate";

        public UnityEvent onItemPlated;
        public UnityEvent onComplete;

        PlateItem[] placed;
        bool complete;

        public bool IsComplete { get { return complete; } }
        public string DishName { get { return dishName; } }

        void Awake()
        {
            placed = new PlateItem[slots.Length];
        }

        void Update()
        {
            UpdateComplete();
            if (complete) return;

            Vector3 boxCenter = zone.transform.TransformPoint(zone.center);
            Vector3 halfExtents = Vector3.Scale(zone.size, zone.transform.lossyScale) * 0.5f;

            foreach (Collider col in Physics.OverlapBox(boxCenter, halfExtents, zone.transform.rotation))
            {
                PlateItem item = col.GetComponentInParent<PlateItem>();
                if (item == null || item.plated || item.IsHeld()) continue;
                if (!string.IsNullOrEmpty(acceptedItem) && item.itemName != acceptedItem) continue;

                int slot = NearestFreeSlot(item.transform.position);
                if (slot < 0) return;

                placed[slot] = item;
                item.Lock(slots[slot]);
                onItemPlated.Invoke();
            }

            UpdateComplete();
        }

        void UpdateComplete()
        {
            int filled = 0;
            foreach (PlateItem p in placed)
                if (p != null) filled++;

            bool nowComplete = slots.Length > 0 && filled == slots.Length;
            if (nowComplete && !complete) onComplete.Invoke();
            complete = nowComplete;
        }

        int NearestFreeSlot(Vector3 position)
        {
            int best = -1;
            float bestDistance = float.MaxValue;
            for (int i = 0; i < slots.Length; i++)
            {
                if (placed[i] != null) continue;
                float d = (slots[i].position - position).sqrMagnitude;
                if (d < bestDistance)
                {
                    bestDistance = d;
                    best = i;
                }
            }
            return best;
        }
    }
}