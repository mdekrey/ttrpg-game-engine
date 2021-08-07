﻿using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using GameEngine.Rules;
using static GameEngine.Generator.ImmutableConstructorExtension;

namespace GameEngine.Generator.Modifiers
{
    public record TemporaryHitPointsFormula(ImmutableList<string> Keywords) : PowerModifierFormula(Keywords, "+Temporary Hit Points")
    {
        public override IEnumerable<ApplicablePowerModifierFormula> GetOptions(AttackProfileBuilder attack, PowerHighLevelInfo powerInfo)
        {
            if (HasModifier(attack)) yield break;
            var amounts = powerInfo.ToolProfile.Abilities.Select(a => (GameDiceExpression)a);
            var targets = new[] { "self", "nearest ally", "an ally within 5 squares" };
            foreach (var amount in amounts)
            {
                foreach (var target in targets)
                {
                    yield return new(new PowerCost(1), BuildModifier(amount, target));
                }
            }

            PowerModifier BuildModifier(GameDiceExpression amount, string target) =>
                new PowerModifier(Name, Build(
                    ("Amount", amount.ToString()),
                    ("Target", target)
                ));
        }

        public override SerializedEffect Apply(SerializedEffect effect, PowerProfile powerProfile, AttackProfile attackProfile, PowerModifier modifier)
        {
            // TODO
            return effect;
        }
    }
}
