using UnityEngine;
using UnityEngine.Events;
using Shared;

namespace Alyssa
{
    public class Plate : MonoBehaviour
    {
        public BoxCollider zone;
        public Transform[] slots;
        public float rejectCooldown = 1f;

        public UnityEvent onItemPlated;
        public UnityEvent onWrongItem;
        [SerializeField] private CookingEvents cookingEvents;

        PlateItem[] placed;
        DishData currentDish;
        bool complete;
        float lastRejectTime = -10f;

        public bool IsComplete { get { return complete; } }
        public DishData Dish { get { return complete ? currentDish : null; } }
        public string DishName { get { return Dish != null ? Dish.displayName : ""; } }

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
                if (item == null || item.plated || item.dish == null || item.IsHeld()) continue;

                if (currentDish != null && item.dish != currentDish)
                {
                    Reject();
                    continue;
                }

                int slot = NearestFreeSlot(item.transform.position);
                if (slot < 0) return;

                currentDish = item.dish;
                placed[slot] = item;
                item.Lock(slots[slot]);
                onItemPlated.Invoke();

                UpdateComplete();
                if (complete) return;
            }
        }

        void Reject()
        {
            if (Time.time - lastRejectTime < rejectCooldown) return;

            lastRejectTime = Time.time;
            onWrongItem.Invoke();
        }

        void UpdateComplete()
        {
            int filled = 0;
            foreach (PlateItem p in placed)
                if (p != null) filled++;

            if (filled == 0) currentDish = null;

            int required = currentDish != null ? Mathf.Min(currentDish.piecesPerDish, slots.Length) : 0;
            bool nowComplete = required > 0 && filled >= required;

            if (nowComplete && !complete && cookingEvents != null) cookingEvents.OnCompleted.Invoke();
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