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
        public IEnumerable<IModifier> AllModifiers() =>
            from set in new IEnumerable<IModifier>[]
            {
                Modifiers
                ,
                from attack in Attacks
                from mod in attack.Modifiers
                select mod
                ,
                from attack in Attacks
                from effect in attack.Effects
                from mod in effect.Modifiers
                select mod
                ,
                from attack in Attacks
                from effect in attack.Effects
                select effect.Target
                ,
                from effect in Effects
                from mod in effect.Modifiers
                select mod
            }
            from mod in set
            select mod;

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
