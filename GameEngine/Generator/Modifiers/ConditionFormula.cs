using System.Collections.Generic;
using System.Collections.Immutable;
using GameEngine.Rules;
using System.Linq;
using static GameEngine.Generator.ImmutableConstructorExtension;
using System;

namespace GameEngine.Generator.Modifiers
{
    public record ConditionFormula() : PowerModifierFormula(ModifierName)
    {
        public const string ModifierName = "Condition";

        public record ConditionOptionKey(Condition Condition, Duration Duration);
        public record ConditionOptionValue(PowerCost Cost, int Chances);

        private static IEnumerable<KeyValuePair<ConditionOptionKey, ConditionOptionValue>> Default(string name, double cost) =>
            new KeyValuePair<ConditionOptionKey, ConditionOptionValue>[]
            {
                new (new (new Condition(name), Duration.EndOfUserNextTurn), new (new PowerCost(Fixed: cost), 5 - (int)(cost * 2))),
                new (new (new Condition(name), Duration.SaveEnds), new (new PowerCost(Fixed: cost * 2), 5 - (int)(cost * 4))),
            };

        private static readonly IReadOnlyList<KeyValuePair<ConditionOptionKey, ConditionOptionValue>> conditions = new[]
        {
            Default("Slowed", 0.5),
            Default("Dazed", 0.5),
            Default("Immobilized", 1),
            Default("Weakened", 1),
            Default("Grants Combat Advantage", 1),
            Default("Blinded", 1),
            OngoingDamage.Options(),
            DefensePenalty.Options(),
        }.SelectMany(e => e).ToArray();

        public override IEnumerable<RandomChances<PowerModifier>> GetOptions(AttackProfileBuilder attack, PowerHighLevelInfo powerInfo)
        {
            if (HasModifier(attack)) yield break;

            foreach (var entry in conditions)
            {
                yield return new(new ConditionModifier(entry.Key.Duration, Build(entry.Key.Condition)), Chances: entry.Value.Chances);
            }
        }

        public record ConditionModifier(Duration Duration, ImmutableList<Condition> Conditions) : PowerModifier(ModifierName)
        {
            public override int GetComplexity() => 1;
            public override PowerCost GetCost() => Conditions.Select(c => conditions.First(sample => sample.Key == new ConditionOptionKey(c, Duration)).Value.Cost).Aggregate((a, b) => a + b);

            public override SerializedEffect Apply(SerializedEffect effect, PowerProfile powerProfile, AttackProfile attackProfile)
            {
                // TODO
                return effect;
            }
        }

        public record Condition(string Name)
        {
        }

        public record OngoingDamage(int Amount) : Condition("Ongoing")
        {
            public static IEnumerable<KeyValuePair<ConditionOptionKey, ConditionOptionValue>> Options() =>
                new KeyValuePair<ConditionOptionKey, ConditionOptionValue>[]
                {
                    new (new (new OngoingDamage(5), Duration.SaveEnds), new (new PowerCost(Fixed: 1), 3)),
                    new (new (new OngoingDamage(10), Duration.SaveEnds), new (new PowerCost(Fixed: 2), 1)),
                };
        }

        public record DefensePenalty(DefenseType? Defense) : Condition("Defense Penalty")
        {
            public static IEnumerable<KeyValuePair<ConditionOptionKey, ConditionOptionValue>> Options() =>
                new KeyValuePair<ConditionOptionKey, ConditionOptionValue>[]
                {
                    new (new (new DefensePenalty(DefenseType.ArmorClass), Duration.EndOfUserNextTurn), new (new PowerCost(0.5), 1)),
                    new (new (new DefensePenalty(DefenseType.Fortitude), Duration.EndOfUserNextTurn), new (new PowerCost(0.5), 1)),
                    new (new (new DefensePenalty(DefenseType.Reflex), Duration.EndOfUserNextTurn), new (new PowerCost(0.5), 1)),
                    new (new (new DefensePenalty(DefenseType.Will), Duration.EndOfUserNextTurn), new (new PowerCost(0.5), 1)),
                    new (new (new DefensePenalty(DefenseType.ArmorClass), Duration.SaveEnds), new (new PowerCost(1), 1)),
                    new (new (new DefensePenalty(DefenseType.Fortitude), Duration.SaveEnds), new (new PowerCost(1), 1)),
                    new (new (new DefensePenalty(DefenseType.Reflex), Duration.SaveEnds), new (new PowerCost(1), 1)),
                    new (new (new DefensePenalty(DefenseType.Will), Duration.SaveEnds), new (new PowerCost(1), 1)),

                    new (new (new DefensePenalty(Defense: null), Duration.EndOfUserNextTurn), new (new PowerCost(1), 1)),
                    new (new (new DefensePenalty(Defense: null), Duration.SaveEnds), new (new PowerCost(2), 1)),
                };
        }
    }
}
