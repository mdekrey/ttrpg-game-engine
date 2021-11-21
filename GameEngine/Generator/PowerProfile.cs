using GameEngine.Generator.Modifiers;
using GameEngine.Rules;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameEngine.Generator
{
    public record PowerProfile(
        PowerFrequency Usage,
        ToolType Tool, ToolRange ToolRange,
        EquatableImmutableList<AttackProfile> Attacks,
        EquatableImmutableList<IPowerModifier> Modifiers,
        EquatableImmutableList<TargetEffect> Effects
    )
    {
        public IEnumerable<IModifier> AllModifiers(bool includeNested)
        {
            var stack = new Stack<IModifier>(Modifiers
                .Concat<IModifier>(from attack in Attacks from mod in attack.AllModifiers() select mod)
                .Concat<IModifier>(from targetEffect in Effects from mod in targetEffect.Modifiers select mod)
            );
            while (stack.TryPop(out var current))
            {
                yield return current;
                if (includeNested)
                    foreach (var entry in current.GetNestedModifiers())
                        stack.Push(entry);
            }
        }

        internal bool Matches(PowerProfile power)
        {
            return Usage == power.Usage
                && Tool == power.Tool
                && ToolRange == power.ToolRange
                && Attacks.Equals(power.Attacks)
                && Modifiers.Where(m => !m.ExcludeFromUniqueness()).SequenceEqual(power.Modifiers.Where(m => !m.ExcludeFromUniqueness()));
        }
    }
}
