using System.Collections.Generic;
using System.Collections.Immutable;
using GameEngine.Rules;
using static GameEngine.Generator.ImmutableConstructorExtension;

namespace GameEngine.Generator.Modifiers
{
    public record RegenerationFormula(ImmutableList<string> Keywords) : PowerModifierFormula(Keywords, "Regeneration")
    {
        public override IEnumerable<ApplicablePowerModifierFormula> GetOptions(AttackProfileBuilder attack, PowerHighLevelInfo powerInfo)
        {
            if (HasModifier(attack)) yield break;
            if (powerInfo.Usage != PowerFrequency.Daily) yield break;
            var amounts = new List<int>();
            if (powerInfo.Level >= 9) amounts.Add(5);
            if (powerInfo.Level >= 19) amounts.Add(10);
            var targets = new[] { "self", "nearest ally", "an ally within 5 squares" };
            foreach (var amount in amounts)
            {
                foreach (var target in targets)
                {
                    yield return new(BuildModifier(new PowerCost(amount / 5.0), amount, target));
                }
            }

            PowerModifier BuildModifier(PowerCost powerCost, GameDiceExpression amount, string target) =>
                new (Name, powerCost, Build(
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
