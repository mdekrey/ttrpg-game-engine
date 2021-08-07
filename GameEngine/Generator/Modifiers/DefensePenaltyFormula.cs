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
            yield return new(new PowerCost(0.5), BuildModifier(DefenseType.ArmorClass, Duration.EndOfUserNextTurn));
            yield return new(new PowerCost(0.5), BuildModifier(DefenseType.Fortitude, Duration.EndOfUserNextTurn));
            yield return new(new PowerCost(0.5), BuildModifier(DefenseType.Reflex, Duration.EndOfUserNextTurn));
            yield return new(new PowerCost(0.5), BuildModifier(DefenseType.Will, Duration.EndOfUserNextTurn));
            yield return new(new PowerCost(1), BuildModifier(DefenseType.ArmorClass, Duration.SaveEnds));
            yield return new(new PowerCost(1), BuildModifier(DefenseType.Fortitude, Duration.SaveEnds));
            yield return new(new PowerCost(1), BuildModifier(DefenseType.Reflex, Duration.SaveEnds));
            yield return new(new PowerCost(1), BuildModifier(DefenseType.Will, Duration.SaveEnds));

            yield return new(new PowerCost(1), BuildModifier(null, Duration.EndOfUserNextTurn));
            yield return new(new PowerCost(2), BuildModifier(null, Duration.SaveEnds));

            PowerModifier BuildModifier(DefenseType? defense, Duration duration) =>
                new PowerModifier(Name, Build(
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
