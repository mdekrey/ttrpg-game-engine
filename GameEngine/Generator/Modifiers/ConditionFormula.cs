using System.Collections.Generic;
using System.Collections.Immutable;
using GameEngine.Rules;
using System.Linq;
using static GameEngine.Generator.ImmutableConstructorExtension;
using System;

namespace GameEngine.Generator.Modifiers
{
    public record ConditionFormula() : AttackModifierFormula(ModifierName)
    {
        public const string ModifierName = "Condition";

        public record ConditionOptionKey(Condition Condition, Duration Duration);
        public record ConditionOptionValue(PowerCost Cost, int Chances);

        private static readonly ImmutableSortedDictionary<string, ImmutableList<string>> subsume =
            new[]
            {
                (Parent: "Immobilized", Children: new[] { "Slowed" }),
                (Parent: "Dazed", Children: new[] { "Grants Combat Advantage" }),
                (Parent: "Unconscious", Children: new[] { "Immobilized", "Dazed", "Slowed", "Weakened", "Blinded" }),
            }.ToImmutableSortedDictionary(e => e.Parent, e => e.Children.ToImmutableList());

        private static readonly ImmutableSortedDictionary<string, double> basicConditions =
            new[]
            {
                (Condition: "Slowed", Cost: 0.5),
                (Condition: "Dazed", Cost: 0.5),
                (Condition: "Immobilized", Cost: 1),
                (Condition: "Weakened", Cost: 1),
                (Condition: "Grants Combat Advantage", Cost: 0.5),
                (Condition: "Blinded", Cost: 1),
            }.ToImmutableSortedDictionary(e => e.Condition, e => e.Cost);
        // TODO - add defense penalties
        private static readonly ImmutableList<Condition> DefenseConditions = new Condition[]
        {
            new DefensePenalty(DefenseType.ArmorClass),
            new DefensePenalty(DefenseType.Fortitude),
            new DefensePenalty(DefenseType.Reflex),
            new DefensePenalty(DefenseType.Will),
        }.ToImmutableList();

        public override IAttackModifier GetBaseModifier(AttackProfileBuilder attack)
        {
            return new ConditionModifier(Duration.EndOfUserNextTurn, ImmutableList<Condition>.Empty);
        }

        public static double DurationMultiplier(Duration duration) =>
            duration == Duration.EndOfEncounter ? 4
            : duration == Duration.SaveEnds ? 2 // Must remain "SaveEnds" if there's a Boost dependent upon it
            : 1;

        public record ConditionModifier(Duration Duration, EquatableImmutableList<Condition> Conditions) : AttackModifier(ModifierName)
        {
            public override int GetComplexity() => 1 + Conditions.Count / 4;
            public override PowerCost GetCost(AttackProfileBuilder builder) => new PowerCost(Fixed: Conditions.Select(c => c.Cost() * DurationMultiplier(Duration)).Sum());

            public override IEnumerable<IAttackModifier> GetUpgrades(AttackProfileBuilder attack, UpgradeStage stage) =>
                from set in new[]
                {
                    from basicCondition in basicConditions.Keys
                    where !Conditions.Select(b => b.Name).Contains(basicCondition)
                    where !GetSubsumed(Conditions).Contains(basicCondition)
                    select this with { Conditions = Filter(Conditions.Items.Add(new Condition(basicCondition))) },

                    from condition in Conditions
                    from upgrade in condition.GetUpgrades(attack.PowerInfo)
                    select this with { Conditions = Filter(Conditions.Items.Remove(condition).Add(upgrade)) },

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
                select mod;

            private static ImmutableList<Condition> Filter(ImmutableList<Condition> conditions)
            {
                var subsumed = GetSubsumed(conditions);
                return conditions.Where(c => !subsumed.Contains(c.Name)).ToImmutableList();
            }

            private static HashSet<string> GetSubsumed(ImmutableList<Condition> conditions) =>
                conditions.Select(c => c.Name).SelectMany(c => subsume.ContainsKey(c)
                                    ? subsume[c]
                                    : Enumerable.Empty<string>()).ToHashSet();

            public override AttackInfoMutator GetAttackInfoMutator() =>
                new(0, (attack, info, index) => attack with
                {
                    // TODO - ongoing/defense penalty/grants combat advantage
                    Hit = attack.Hit + ", and the target is " + PowerProfileTextGeneration.OxfordComma(Conditions.Select(c => c.Name.ToLower()).ToArray())
                        + Duration switch 
                        {
                            Duration.EndOfUserNextTurn => " until the end of your next turn.",
                            Duration.SaveEnds => " (save ends.)",
                            Duration.EndOfEncounter => " until the end of the encounter.",
                            _ => throw new NotImplementedException(),
                        },
                });
        }

        public record Condition(string Name)
        {
            public virtual double Cost() => basicConditions[Name];
            public virtual IEnumerable<Condition> GetUpgrades(PowerHighLevelInfo powerInfo) =>
                Enumerable.Empty<Condition>();
        }

        public record OngoingDamage(int Amount) : Condition("Ongoing")
        {
            public override double Cost() => Amount / 5;

            public override IEnumerable<Condition> GetUpgrades(PowerHighLevelInfo powerInfo)
            {
                if (Amount < 15)
                    yield return new OngoingDamage(Amount + 5);
            }
        }

        public record DefensePenalty(DefenseType? Defense) : Condition("Defense Penalty")
        {
            public override double Cost() => Defense == null ? 1 : 0.5;

            public override IEnumerable<Condition> GetUpgrades(PowerHighLevelInfo powerInfo)
            {
                if (Defense != null)
                    yield return new DefensePenalty((DefenseType?)null);
            }
        }
    }
}
