using GameEngine.Generator.Modifiers;
using GameEngine.Rules;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace GameEngine.Generator
{
    public record PowerProfile(
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
                        stack.Push(current.Get(entry));
            }
        }

        internal bool Matches(PowerProfile power)
        {
            return Attacks.Equals(power.Attacks)
                && Modifiers.Where(m => !m.ExcludeFromUniqueness()).SequenceEqual(power.Modifiers.Where(m => !m.ExcludeFromUniqueness()));
        }
    }

    public record PowerInfo(
        PowerFrequency Usage,
        ToolType ToolType, ToolRange ToolRange,
        int Level,
        EquatableImmutableList<Ability> Abilities,
        EquatableImmutableList<string> PossibleRestrictions) : IPowerInfo
    {
        ImmutableList<Ability> IPowerInfo.Abilities => Abilities.Items;
        ImmutableList<string> IPowerInfo.PossibleRestrictions => PossibleRestrictions.Items;
    }
}
