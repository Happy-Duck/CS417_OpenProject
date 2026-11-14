using System.Collections.Generic;

namespace Shared{
    public static class ResourceManager2
    {
        // Adds or subtracts an amount without going below the resource's minimum.
        public static void Add(this Resource resource, float amount)
        {
            resource.Value += amount;
        }

        // tells you if the player has enough of a resource
        public static bool CanAfford(this Resource resource, float amount)
        {
            return amount<=0 || resource.Value - amount >= resource.minValue;
        }

        // tells you if the player has enough of every resource in a cost list
        public static bool CanAfford(this IReadOnlyDictionary<Resource, float> costs)
        {
            foreach (var cost in costs)
                if (!cost.Key.CanAfford(cost.Value)) return false;
            return true;
        }

		// spends some amount of a resource if they can afford it, otherwise it returns false and changes nothing
		public static bool TrySpend(this Resource resource, float amount) {
			if (!CanAfford(resource, amount)) return false;
			if (amount > 0) resource.Add(-amount);
			return true;
		}

        // spends several resources at once if they can afford all of them, otherwise it returns false and changes nothing
        public static bool TrySpend(this IReadOnlyDictionary<Resource, float> costs)
        {
            if (!costs.CanAfford()) return false;
            foreach (var cost in costs)
                if (cost.Value > 0) cost.Key.Add(-cost.Value);
            return true;
        }
    }
}
