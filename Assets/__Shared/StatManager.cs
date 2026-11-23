using System.Collections.Generic;

namespace Shared{
    public static class StatManager
    {
        public static void Add(this Stat stat, float amount)
        {
            stat.Value += amount;
        }

        public static void SetValue(this Stat stat, float value)
        {
            stat.Value = value;
        }
    }
}
