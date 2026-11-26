using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Alyssa
{
    public class RollBuilder : MonoBehaviour
    {
        public BoxCollider zone;
        public Transform[] slots;
        public RollData[] recipes;
        public Nori nori;
        public RiceTarget rice;
        public float rejectCooldown = 1f;

        public UnityEvent onIngredientAdded;
        public UnityEvent onWrongIngredient;
        public UnityEvent onNotReady;       // nori not on mat, or no rice yet
        public UnityEvent onRollComplete;

        RollIngredient[] slotItems;
        RollData result;
        float lastRejectTime = -10f;

        public bool IsComplete { get { return result != null; } }
        public RollData Roll { get { return result; } }

        void Awake()
        {
            slotItems = new RollIngredient[slots.Length];
            if (nori == null) nori = GetComponentInParent<Nori>();
            if (rice == null) rice = GetComponentInParent<RiceTarget>();
        }

        bool Ready()
        {
            return nori != null && nori.OnMat && rice != null && rice.HasRice;
        }

        void Update()
        {
            if (result != null) return;

            Vector3 boxCenter = zone.transform.TransformPoint(zone.center);
            Vector3 halfExtents = Vector3.Scale(zone.size, zone.transform.lossyScale) * 0.5f;

            foreach (Collider col in Physics.OverlapBox(boxCenter, halfExtents, zone.transform.rotation))
            {
                RollIngredient item = col.GetComponentInParent<RollIngredient>();
                if (item == null || item.Placed || item.ingredient == null || item.IsHeld()) continue;

                if (!Ready())
                {
                    Reject(onNotReady);
                    continue;
                }

                if (!Accepts(item.ingredient))
                {
                    Reject(onWrongIngredient);
                    continue;
                }

                int slot = NearestFreeSlot(item.transform.position);
                if (slot < 0) return;

                slotItems[slot] = item;
                item.Lock(slots[slot]);
                onIngredientAdded.Invoke();

                result = FindCompleteRecipe();
                if (result != null)
                {
                    onRollComplete.Invoke();
                    return;
                }
            }
        }

        // An ingredient is accepted if the contents plus it still fit inside at least one recipe
        bool Accepts(IngredientData incoming)
        {
            List<IngredientData> contents = Contents();
            contents.Add(incoming);

            foreach (RollData r in recipes)
                if (r != null && FitsInside(contents, r.ingredients)) return true;
            return false;
        }

        RollData FindCompleteRecipe()
        {
            List<IngredientData> contents = Contents();

            foreach (RollData r in recipes)
                if (r != null && r.ingredients.Length == contents.Count && FitsInside(contents, r.ingredients))
                    return r;
            return null;
        }

        static bool FitsInside(List<IngredientData> contents, IngredientData[] recipe)
        {
            List<IngredientData> remaining = new List<IngredientData>(recipe);
            foreach (IngredientData i in contents)
                if (!remaining.Remove(i)) return false;
            return true;
        }

        List<IngredientData> Contents()
        {
            List<IngredientData> list = new List<IngredientData>();
            foreach (RollIngredient p in slotItems)
                if (p != null) list.Add(p.ingredient);
            return list;
        }

        void Reject(UnityEvent e)
        {
            if (Time.time - lastRejectTime < rejectCooldown) return;
            lastRejectTime = Time.time;
            e.Invoke();
        }

        int NearestFreeSlot(Vector3 position)
        {
            int best = -1;
            float bestDistance = float.MaxValue;
            for (int i = 0; i < slots.Length; i++)
            {
                if (slotItems[i] != null) continue;
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