using System.Collections.Generic;
using System.Linq;

namespace GameEngine.Generator
{
    public static class PowerCostExtensions
    {
        public static PowerCost Sum(this IEnumerable<PowerCost> costs)
        {
            return costs.DefaultIfEmpty(PowerCost.Empty).Aggregate((a, b) => a + b);
        }
    }
}
