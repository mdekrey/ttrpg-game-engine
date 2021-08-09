using System.Collections.Generic;
using System.Collections.Immutable;
using GameEngine.Rules;
using static GameEngine.Generator.ImmutableConstructorExtension;

namespace GameEngine.Generator.Modifiers
{
    public record DefenseBoostFormula(ImmutableList<string> Keywords) : PowerModifierFormula(Keywords, "+2 to Defense")
    {
        public override IEnumerable<ApplicablePowerModifierFormula> GetOptions(AttackProfileBuilder attack, PowerHighLevelInfo powerInfo)
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

            PowerModifierBuilder BuildModifier(PowerCost powerCost, DefenseType defense, Duration duration, string target) =>
                new (Name, powerCost, Build(
                    ("Defense", defense.ToString("g")),
                    ("Amount", "+2"),
                    ("Duration", duration.ToString("g")),
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
