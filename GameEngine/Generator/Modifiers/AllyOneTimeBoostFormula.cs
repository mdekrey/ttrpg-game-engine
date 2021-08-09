﻿using System.Collections.Generic;
using System.Collections.Immutable;
using GameEngine.Rules;
using static GameEngine.Generator.ImmutableConstructorExtension;

namespace GameEngine.Generator.Modifiers
{
    public record AllyOneTimeBoostFormula(ImmutableList<string> Keywords, string Name, PowerCost Cost) : PowerModifierFormula(Keywords, Name)
    {
        public override IEnumerable<RandomChances<PowerModifier>> GetOptions(AttackProfileBuilder attack, PowerHighLevelInfo powerInfo)
        {
            if (HasModifier(attack)) yield break;
            var targets = new[] { "self", "nearest ally", "an ally within 5 squares" };
            foreach (var target in targets)
            {
                yield return new(BuildModifier(target, Cost));
            }

            AllyOneTimeBoost BuildModifier(string target, PowerCost cost) =>
                new(Name, cost, target);
        }

        public record AllyOneTimeBoost(string Name, PowerCost Cost, string Target) : PowerModifier(Name)
        {
            public override PowerCost GetCost() => Cost;

            public override SerializedEffect Apply(SerializedEffect effect, PowerProfile powerProfile, AttackProfile attackProfile)
            {
                // TODO
                return effect;
            }
        }
    }
}
