using System.Collections.Generic;
using System.Linq;

namespace GameEngine.Generator.Modifiers
{
    public static class ModifierHelpers
    {
        public static int GetComplexity(this IEnumerable<IModifier> modifiers, PowerHighLevelInfo powerInfo) => modifiers.Select(m => m.GetComplexity(powerInfo)).DefaultIfEmpty(0).Sum();
    }
}
