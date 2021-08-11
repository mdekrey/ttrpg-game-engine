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

        private static readonly ImmutableSortedDictionary<string, double> basicConditions =
            new[]
            {
                (Condition: "Slowed", Cost: 0.5), // TODO - do not stack this with Immobilized
                (Condition: "Dazed", Cost: 0.5),
                (Condition: "Immobilized", Cost: 1),
                (Condition: "Weakened", Cost: 1),
                (Condition: "Grants Combat Advantage", Cost: 0.5), // TODO - do not stack this with Dazed
                (Condition: "Blinded", Cost: 1),
            }.ToImmutableSortedDictionary(e => e.Condition, e => e.Cost);
        private static readonly ImmutableList<Condition> DefenseConditions = new Condition[]
        {
            new DefensePenalty(DefenseType.ArmorClass),
            new DefensePenalty(DefenseType.Fortitude),
            new DefensePenalty(DefenseType.Reflex),
            new DefensePenalty(DefenseType.Will),
        }.ToImmutableList();

        public override IEnumerable<RandomChances<PowerModifier>> GetOptions(AttackProfileBuilder attack)
        {
            if (HasModifier(attack)) yield break;

            foreach (var condition in basicConditions.Keys.Select(e => new Condition(e)).Concat(DefenseConditions))
            {
                yield return new(new ConditionModifier(Duration.EndOfUserNextTurn, Build(condition)));
            }
            yield return new(new ConditionModifier(Duration.SaveEnds, Build<Condition>(new OngoingDamage(5))));
        }

        public static double DurationMultiplier(Duration duration) =>
            duration == Duration.EndOfEncounter ? 4
            : duration == Duration.SaveEnds ? 2 // Must remain "SaveEnds" if there's a Boost dependent upon it
            : 1;

        public record ConditionModifier(Duration Duration, ImmutableList<Condition> Conditions) : PowerModifier(ModifierName)
        {
            public override int GetComplexity() => 1;
            public override PowerCost GetCost() => new PowerCost(Fixed: Conditions.Select(c => c.Cost() * DurationMultiplier(Duration)).Sum());

            public override IEnumerable<RandomChances<PowerModifier>> GetUpgrades(AttackProfileBuilder attack) =>
                from set in new[]
                {
                    from basicCondition in basicConditions.Keys
                    where !Conditions.Select(b => b.Name).Contains(basicCondition)
                    select this with { Conditions = Conditions.Add(new Condition(basicCondition)) },

                    from condition in Conditions
                    from upgrade in condition.GetUpgrades(attack)
                    select this with { Conditions = Conditions.Remove(condition).Add(upgrade) },

                    from duration in new[] { Duration.SaveEnds, Duration.EndOfEncounter }
                    where attack.Modifiers.OfType<BoostFormula.BoostModifier>().FirstOrDefault() is not BoostFormula.BoostModifier { Duration: Duration.SaveEnds }
                    where duration > Duration
                    where duration switch
                    {
                        Duration.EndOfEncounter => attack.PowerInfo.Usage == PowerFrequency.Daily,
                        Duration.SaveEnds => true,
                        _ => false,
                    }
                    select this with { Duration = duration },
                }
                from mod in set
                select new RandomChances<PowerModifier>(mod);

            public override SerializedEffect Apply(SerializedEffect effect, PowerProfile powerProfile, AttackProfile attackProfile)
            {
                // TODO
                return effect;
            }
        }

        public record Condition(string Name)
        {
            public virtual double Cost() => basicConditions[Name];
            public virtual IEnumerable<Condition> GetUpgrades(AttackProfileBuilder attack) =>
                Enumerable.Empty<Condition>();
        }

        public record OngoingDamage(int Amount) : Condition("Ongoing")
        {
            public override double Cost() => Amount / 5;

            public override IEnumerable<Condition> GetUpgrades(AttackProfileBuilder attack)
            {
                if (Amount < 15)
                    yield return new OngoingDamage(Amount + 5);
            }
        }

        public record DefensePenalty(DefenseType? Defense) : Condition("Defense Penalty")
        {
            public override double Cost() => Defense == null ? 1 : 0.5;

            public override IEnumerable<Condition> GetUpgrades(AttackProfileBuilder attack)
            {
                if (Defense != null)
                    yield return new DefensePenalty((DefenseType?)null);
            }
        }
    }
}
