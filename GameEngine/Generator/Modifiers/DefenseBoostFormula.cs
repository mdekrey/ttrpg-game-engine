using System.Collections.Generic;
using System.Collections.Immutable;
using GameEngine.Rules;
using static GameEngine.Generator.ImmutableConstructorExtension;

namespace GameEngine.Generator.Modifiers
{
    public record DefenseBoostFormula(ImmutableList<string> Keywords) : PowerModifierFormula(Keywords, ModifierName)
    {
        public const string ModifierName = "+2 to Defense";

        public override IEnumerable<RandomChances<PowerModifier>> GetOptions(AttackProfileBuilder attack, PowerHighLevelInfo powerInfo)
        {
            if (HasModifier(attack)) yield break;
            var defenses = new[] { DefenseType.ArmorClass, DefenseType.Fortitude, DefenseType.Reflex, DefenseType.Will };
            var targets = new[] { "self", "nearest ally", "an ally within 5 squares" };
            foreach (var defense in defenses)
            {
                foreach (var target in targets)
                {
                    yield return new(BuildModifier(new PowerCost(0.5), defense, Duration.EndOfUserNextTurn, target));
                    if (powerInfo.Usage == PowerFrequency.Daily)
                        yield return new(BuildModifier(new PowerCost(Multiplier: 0.5), defense, Duration.EndOfEncounter, target));
                }
            }

            DefenseBoost BuildModifier(PowerCost powerCost, DefenseType defense, Duration duration, string target) =>
                new(powerCost, defense, 2, duration, target);
        }

        public record DefenseBoost(PowerCost Cost, DefenseType Defense, GameDiceExpression Amount, Duration Duration, string Target) : PowerModifier(ModifierName)
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
