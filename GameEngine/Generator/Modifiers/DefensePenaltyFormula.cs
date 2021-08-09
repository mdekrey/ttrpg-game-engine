using System.Collections.Generic;
using System.Collections.Immutable;
using GameEngine.Rules;
using static GameEngine.Generator.ImmutableConstructorExtension;

namespace GameEngine.Generator.Modifiers
{
    public record DefensePenaltyFormula(ImmutableList<string> Keywords) : PowerModifierFormula(Keywords, ModifierName)
    {
        public const string ModifierName = "-2 to Defense";
        public DefensePenaltyFormula(params string[] keywords) : this(keywords.ToImmutableList()) { }

        public override IEnumerable<RandomChances<PowerModifier>> GetOptions(AttackProfileBuilder attack, PowerHighLevelInfo powerInfo)
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

            DefensePenalty BuildModifier(PowerCost powerCost, DefenseType? defense, Duration duration) =>
                new (powerCost, defense, duration);
        }

        public record DefensePenalty(PowerCost Cost, DefenseType? Defense, Duration Duration) : PowerModifier(ModifierName)
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
