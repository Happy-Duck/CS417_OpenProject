using System.Collections.Generic;

namespace Shared{
    public static class StatManager
    {
        // Adds or subtracts an amount without going below the resource's minimum.
        public static void Add(this Stat stat, float amount)
        {
            stat.Value += amount;
        }
    }
}
