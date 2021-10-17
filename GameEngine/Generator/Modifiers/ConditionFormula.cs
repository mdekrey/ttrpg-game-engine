using System.Collections.Generic;
using System.Collections.Immutable;
using GameEngine.Rules;
using System.Linq;
using static GameEngine.Generator.ImmutableConstructorExtension;
using System;
using static GameEngine.Generator.ProseHelpers;

namespace GameEngine.Generator.Modifiers
{
    public record ConditionFormula() : TargetEffectFormula(ModifierName)
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

        private static readonly ImmutableSortedDictionary<string, (double cost, string verb, string effect)> basicConditions =
            new[]
            {
                (Condition: "Slowed", Cost: 0.5, Verb: "is", Effect: "Slowed"),
                (Condition: "Dazed", Cost: 0.5, Verb: "is", Effect: "Dazed"),
                (Condition: "Immobilized", Cost: 1, Verb: "is", Effect: "Immobilized"),
                (Condition: "Weakened", Cost: 1, Verb: "is", Effect: "Weakened"),
                (Condition: "Grants Combat Advantage", Cost: 0.5, Verb: "grants", Effect: "Combat Advantage"),
                (Condition: "Blinded", Cost: 1, Verb: "is", Effect: "Blinded"),
            }.ToImmutableSortedDictionary(e => e.Condition, e => (e.Cost, e.Verb, e.Effect));
        // TODO - add defense penalties
        private static readonly ImmutableList<Condition> DefenseConditions = new Condition[]
        {
            new DefensePenalty(DefenseType.ArmorClass),
            new DefensePenalty(DefenseType.Fortitude),
            new DefensePenalty(DefenseType.Reflex),
            new DefensePenalty(DefenseType.Will),
        }.ToImmutableList();

        public override IEnumerable<ITargetEffectModifier> GetBaseModifiers(UpgradeStage stage, TargetEffectBuilder target, PowerProfileBuilder power)
        {
            return new ConditionModifier(ImmutableList<Condition>.Empty).GetUpgrades(stage, target, power);
        }

        public static double DurationMultiplier(Duration duration) =>
            duration == Duration.EndOfEncounter ? 4
            : duration == Duration.SaveEnds ? 2 // Must remain "SaveEnds" if there's a Boost dependent upon it
            : 1;

        public record ConditionModifier(EquatableImmutableList<Condition> Conditions) : TargetEffectModifier(ModifierName)
        {
            public override Target ValidTargets() => Target.Enemy;
            public override int GetComplexity(PowerHighLevelInfo powerInfo) => (Conditions.Count + 2) / 3;
            public override PowerCost GetCost(TargetEffectBuilder builder, PowerProfileBuilder power) => 
                new PowerCost(Fixed: Conditions.Select(c => c.Cost() * DurationMultiplier(power.GetDuration())).Sum());
            public override bool IsPlaceholder() => Conditions.Count == 0;
            public override bool UsesDuration() => true;

            public override IEnumerable<ITargetEffectModifier> GetUpgrades(UpgradeStage stage, TargetEffectBuilder target, PowerProfileBuilder power) =>
                (stage < UpgradeStage.Standard) ? Enumerable.Empty<ITargetEffectModifier>() :
                from set in new[]
                {
                    from basicCondition in basicConditions.Keys
                    where !Conditions.Select(b => b.Name).Contains(basicCondition)
                    where !GetSubsumed(Conditions).Contains(basicCondition)
                    select this with { Conditions = Filter(Conditions.Items.Add(new Condition(basicCondition))) },

                    from condition in Conditions
                    from upgrade in condition.GetUpgrades(target.PowerInfo)
                    select this with { Conditions = Filter(Conditions.Items.Remove(condition).Add(upgrade)) },
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

            //public override AttackInfoMutator? GetAttackInfoMutator(PowerProfile power) =>
            //    new(0, (attack, info, index) => attack with
            //    {
            //        HitParts = attack.HitParts.Add("the target "
            //            + OxfordComma((from condition in Conditions
            //                                                      group condition.Effect().ToLower() by condition.Verb() into verbGroup
            //                                                      select verbGroup.Key + " " + OxfordComma(verbGroup.ToArray())).ToArray())
            //            + EffectDurationFormula.GetDuration(power) switch
            //            {
            //                Duration.EndOfUserNextTurn => " until the end of your next turn",
            //                Duration.SaveEnds => " (save ends)",
            //                Duration.EndOfEncounter => " until the end of the encounter",
            //                _ => throw new NotImplementedException(),
            //            }),
            //    });

            public bool DurationAffected() => Conditions.Any();
            public bool CanSaveEnd() => Conditions.Any();
        }

        public record Condition(string Name)
        {
            public virtual double Cost() => basicConditions[Name].cost;
            public virtual IEnumerable<Condition> GetUpgrades(PowerHighLevelInfo powerInfo) =>
                Enumerable.Empty<Condition>();
            public virtual string Verb() => basicConditions[Name].verb;
            public virtual string Effect() => basicConditions[Name].effect;
        }

        public record OngoingDamage(int Amount) : Condition("Ongoing")
        {
            public override double Cost() => Amount / 5;

            public override IEnumerable<Condition> GetUpgrades(PowerHighLevelInfo powerInfo)
            {
                if (Amount < 15)
                    yield return new OngoingDamage(Amount + 5);
            }
            public override string Verb() => "takes";
            public override string Effect() => $"ongoing {Amount}";
        }

        public record DefensePenalty(DefenseType? Defense) : Condition("Defense Penalty")
        {
            public override double Cost() => Defense == null ? 1 : 0.5;

            public override IEnumerable<Condition> GetUpgrades(PowerHighLevelInfo powerInfo)
            {
                if (Defense != null)
                    yield return new DefensePenalty((DefenseType?)null);
            }
            public override string Verb() => "takes";
            public override string Effect() => $"a -2 penalty to {Defense?.ToText() switch { string s => "its " + s, _ => "all defenses" }}";
        }
    }
}
