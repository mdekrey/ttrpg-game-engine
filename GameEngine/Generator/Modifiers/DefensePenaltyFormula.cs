using System.Collections.Generic;
using System.Collections.Immutable;
using GameEngine.Rules;
using static GameEngine.Generator.ImmutableConstructorExtension;

namespace GameEngine.Generator.Modifiers
{
    public record DefensePenaltyFormula(ImmutableList<string> Keywords) : PowerModifierFormula(Keywords, "-2 to Defense")
    {
        public DefensePenaltyFormula(params string[] keywords) : this(keywords.ToImmutableList()) { }

        public override IEnumerable<ApplicablePowerModifierFormula> GetOptions(AttackProfileBuilder attack, PowerHighLevelInfo powerInfo)
        {
            if (HasModifier(attack)) yield break;
            yield return new(BuildModifier(new PowerCost(0.5), DefenseType.ArmorClass, Duration.EndOfUserNextTurn));
            yield return new(BuildModifier(new PowerCost(0.5), DefenseType.Fortitude, Duration.EndOfUserNextTurn));
            yield return new(BuildModifier(new PowerCost(0.5), DefenseType.Reflex, Duration.EndOfUserNextTurn));
            yield return new(BuildModifier(new PowerCost(0.5), DefenseType.Will, Duration.EndOfUserNextTurn));
            yield return new(BuildModifier(new PowerCost(1), DefenseType.ArmorClass, Duration.SaveEnds));
            yield return new(BuildModifier(new PowerCost(1), DefenseType.Fortitude, Duration.SaveEnds));
            yield return new(BuildModifier(new PowerCost(1), DefenseType.Reflex, Duration.SaveEnds));
            yield return new(BuildModifier(new PowerCost(1), DefenseType.Will, Duration.SaveEnds));

            yield return new(BuildModifier(new PowerCost(1), null, Duration.EndOfUserNextTurn));
            yield return new(BuildModifier(new PowerCost(2), null, Duration.SaveEnds));

            PowerModifierBuilder BuildModifier(PowerCost powerCost, DefenseType? defense, Duration duration) =>
                new (Name, powerCost, Build(
                    ("Defense", defense?.ToString("g") ?? "All"),
                    ("Duration", duration.ToString("g"))
                ));
        }

        public override SerializedEffect Apply(SerializedEffect effect, PowerProfile powerProfile, AttackProfile attackProfile, PowerModifier modifier)
        {
            // TODO
            return effect;
        }
    }

}
