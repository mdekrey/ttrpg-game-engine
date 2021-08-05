using System.Collections.Generic;
using System.Collections.Immutable;
using GameEngine.Rules;
using static GameEngine.Generator.ImmutableConstructorExtension;

namespace GameEngine.Generator.Modifiers
{
    public record RegenerationFormula(ImmutableList<string> Keywords) : PowerModifierFormula(Keywords, "Regeneration")
    {
        public override IEnumerable<ApplicablePowerModifierFormula> GetApplicable(AttackProfileBuilder attack, PowerHighLevelInfo powerInfo)
        {
            if (HasModifier(attack)) yield break;
            if (powerInfo.Usage != PowerFrequency.Daily) yield break;
            var amounts = new[] { 5, 10 };
            var targets = new[] { "self", "nearest ally", "an ally within 5 squares" };
            foreach (var amount in amounts)
            {
                foreach (var target in targets)
                {
                    yield return new(new PowerCost(amount / 5.0), BuildModifier(amount, target));
                }
            }

            PowerModifier BuildModifier(GameDiceExpression amount, string target) =>
                new PowerModifier(Name, Build(
                    ("Amount", amount.ToString()),
                    ("Duration", nameof(Duration.EndOfEncounter)),
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
