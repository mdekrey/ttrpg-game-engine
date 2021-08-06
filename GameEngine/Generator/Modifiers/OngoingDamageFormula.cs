﻿using System.Collections.Generic;
using System.Collections.Immutable;
using GameEngine.Rules;
using static GameEngine.Generator.ImmutableConstructorExtension;

namespace GameEngine.Generator.Modifiers
{
    public record OngoingDamageFormula(ImmutableList<string> Keywords) : PowerModifierFormula(Keywords, "Ongoing Damage")
    {
        public override IEnumerable<ApplicablePowerModifierFormula> GetApplicable(AttackProfileBuilder attack, PowerHighLevelInfo powerInfo)
        {
            if (HasModifier(attack)) yield break;

            var amounts = new[] { 5, 10 };
            foreach (var amount in amounts)
            {
                yield return new(new PowerCost(amount / 2.5), BuildModifier(amount));
            }

            PowerModifier BuildModifier(GameDiceExpression amount) =>
                new PowerModifier(Name, Build(
                    ("Amount", amount.ToString())
                ));
        }

        public override SerializedEffect Apply(SerializedEffect effect, PowerProfile powerProfile, AttackProfile attackProfile, PowerModifier modifier)
        {
            throw new System.NotImplementedException();
        }
    }
}