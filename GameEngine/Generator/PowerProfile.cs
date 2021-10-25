﻿using GameEngine.Generator.Modifiers;
using GameEngine.Rules;
using System;
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