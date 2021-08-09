using System.Collections.Generic;
using System.Collections.Immutable;
using GameEngine.Rules;
using static GameEngine.Generator.ImmutableConstructorExtension;

namespace GameEngine.Generator.Modifiers
{
    public record RegenerationFormula() : PowerModifierFormula(ModifierName)
    {
        public const string ModifierName = "Regeneration";
        public override IEnumerable<RandomChances<PowerModifier>> GetOptions(AttackProfileBuilder attack, PowerHighLevelInfo powerInfo)
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

            RegenerationModifier BuildModifier(PowerCost powerCost, GameDiceExpression amount, string target) =>
                new(powerCost, amount, target, Duration.EndOfEncounter);
        }

        public record RegenerationModifier(PowerCost Cost, GameDiceExpression Amount, string Target, Duration Duration) : PowerModifier(ModifierName)
        {
            public override int GetComplexity() => 1;

            public override PowerCost GetCost() => Cost;

            public override SerializedEffect Apply(SerializedEffect effect, PowerProfile powerProfile, AttackProfile attackProfile)
            {
                // TODO
                return effect;
            }
        }
    }
}
