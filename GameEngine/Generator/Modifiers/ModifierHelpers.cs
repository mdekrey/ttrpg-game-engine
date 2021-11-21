using GameEngine.Generator.Context;
using System.Collections.Generic;
using System.Linq;

namespace GameEngine.Generator.Modifiers
{
    public static class ModifierHelpers
    {
        public static int GetComplexity(this IEnumerable<IModifier> modifiers, PowerContext powerContext) => modifiers.Select(m => m.GetComplexity(powerContext)).DefaultIfEmpty(0).Sum();
    }
}
