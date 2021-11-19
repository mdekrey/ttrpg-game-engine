﻿using System.Collections.Generic;
using System.Collections.Immutable;
using GameEngine.Rules;
using System.Linq;
using static System.Collections.Immutable.ImmutableList<string>;
using System;
using static GameEngine.Generator.ProseHelpers;
using GameEngine.Generator.Text;

namespace GameEngine.Generator.Modifiers
{
    public record ConditionFormula() : IEffectFormula, IPowerModifierFormula
    {
        private static readonly Lens<PowerProfileBuilder, ImmutableList<AttackProfile>> attacksLens = Lens<PowerProfileBuilder>.To(p => p.Attacks, (p, a) => p with { Attacks = a });
        private static readonly Lens<PowerProfileBuilder, ImmutableList<TargetEffect>> effectsLens = Lens<PowerProfileBuilder>.To(p => p.Effects, (p, e) => p with { Effects = e });
        private static readonly Lens<AttackProfile, ImmutableList<TargetEffect>> attackToEffectLens = Lens<AttackProfile>.To(p => p.Effects.Items, (p, e) => p with { Effects = e });

        public const string ModifierName = "Condition";

        public record ConditionOptionKey(Condition Condition, Duration Duration);
        public record ConditionOptionValue(PowerCost Cost, int Chances);

        public record ConditionDefinition(string Name, double Cost, string OtherVerb, string SelfVerb, string Effect, bool AllowDirectApplication, bool AllowAfterEfect, ImmutableList<string> Subsumes)
        {
            public ConditionDefinition(string Name, double Cost, string OtherVerb, string SelfVerb, string Effect, bool AllowDirectApplication, bool AllowAfterEfect)
                : this(Name: Name, Cost: Cost, OtherVerb: OtherVerb, SelfVerb: SelfVerb, Effect: Effect, AllowDirectApplication: AllowDirectApplication, AllowAfterEfect, Subsumes: Empty)
            {
            }
        }

        private static readonly ImmutableSortedDictionary<string, ConditionDefinition> basicConditions =
            new ConditionDefinition[]
            {
                new (Name: "Blinded",
                    Subsumes: Empty.Add("Grants Combat Advantage"),
                    AllowDirectApplication: true, AllowAfterEfect: true, Cost: 1,
                    OtherVerb: "is", SelfVerb: "are", Effect: "Blinded"
                ),
                new (Name: "Dazed",
                    Subsumes: Empty.Add("Grants Combat Advantage"),
                    AllowDirectApplication: true, AllowAfterEfect: true, Cost: 0.5,
                    OtherVerb: "is", SelfVerb: "are", Effect: "Dazed"
                ),
                new (Name: "Deafened",
                    AllowDirectApplication: true, AllowAfterEfect: true, Cost: 0.5,
                    OtherVerb: "is", SelfVerb: "are", Effect: "Deafened"
                ),
                new (Name: "Dominated",
                    Subsumes: Empty.AddRange(new[] { "Dazed", "Grants Combat Advantage" }),
                    AllowDirectApplication: false, AllowAfterEfect: true, Cost: 2, // TODO - check cost
                    OtherVerb: "is", SelfVerb: "are", Effect: "Dominated"
                ),
                new (Name: "Dying",
                    Subsumes: Empty.AddRange(new[] { "Unconscious", "Immobilized", "Slowed", "Weakened", "Blinded", "Deafened", "Stunned", "Dazed", "Marked", "Surprised", "Grants Combat Advantage" }),
                    AllowDirectApplication: false, AllowAfterEfect: false, Cost: 4,
                    OtherVerb: "is", SelfVerb: "are", Effect: "Dying"
                ),
                new (Name: "Grants Combat Advantage",
                    AllowDirectApplication: true, AllowAfterEfect: false, Cost: 0.5,
                    OtherVerb: "grants", SelfVerb: "grant", Effect: "Combat Advantage"
                ),
                new (Name: "Helpless",
                    Subsumes: Empty.AddRange(new[] { "Grants Combat Advantage" }),
                    AllowDirectApplication: false, AllowAfterEfect: false, Cost: 1.5, // TODO - check cost
                    OtherVerb: "is", SelfVerb: "are", Effect: "Helpless"
                ),
                new (Name: "Immobilized",
                    Subsumes: Empty.AddRange(new[] { "Slowed" }),
                    AllowDirectApplication: false, AllowAfterEfect: true, Cost: 1,
                    OtherVerb: "is", SelfVerb: "are", Effect: "Immobilized"
                ),
                new (Name: "Marked",
                    AllowDirectApplication: true, AllowAfterEfect: false, Cost: 0.5, // TODO - check cost
                    OtherVerb: "is", SelfVerb: "are", Effect: "Marked"
                ),
                new (Name: "Petrified",
                    Subsumes: Empty.AddRange(new[] { "Immobilized", "Slowed" }),
                    AllowDirectApplication: false, AllowAfterEfect: true, Cost: 2, // TODO - check cost
                    OtherVerb: "is", SelfVerb: "are", Effect: "Petrified"
                ),
                new (Name: "Restrained",
                    Subsumes: Empty.AddRange(new[] { "Immobilized", "Slowed" }),
                    AllowDirectApplication: true, AllowAfterEfect: false, Cost: 2, // TODO - check cost
                    OtherVerb: "is", SelfVerb: "are", Effect: "Restrained"
                ),
                new (Name: "Slowed",
                    AllowDirectApplication: true, AllowAfterEfect: true, Cost: 0.5,
                    OtherVerb: "is", SelfVerb: "are", Effect: "Slowed"
                ),
                new (Name: "Stunned",
                    Subsumes: Empty.AddRange(new[] { "Dazed", "Marked", "Surprised", "Grants Combat Advantage" }),
                    AllowDirectApplication: true, AllowAfterEfect: true, Cost: 1, // TODO - check cost
                    OtherVerb: "is", SelfVerb: "are", Effect: "Stunned"
                ),
                new (Name: "Surprised",
                    Subsumes: Empty.AddRange(new[] { "Dazed", "Grants Combat Advantage" }),
                    AllowDirectApplication: true, AllowAfterEfect: false, Cost: 1, // TODO - check cost
                    OtherVerb: "is", SelfVerb: "are", Effect: "Surprised"
                ),
                new (Name: "Unconscious",
                    Subsumes: Empty.AddRange(new[] { "Immobilized", "Slowed", "Weakened", "Blinded", "Deafened", "Stunned", "Dazed", "Marked", "Surprised", "Grants Combat Advantage" }),
                    AllowDirectApplication: false, AllowAfterEfect: true, Cost: 2,
                    OtherVerb: "becomes", SelfVerb: "become", Effect: "Unconscious"
                ),
                new (Name: "Weakened",
                    AllowDirectApplication: true, AllowAfterEfect: true, Cost: 1,
                    OtherVerb: "is", SelfVerb: "are", Effect: "Weakened"
                ),
            }.ToImmutableSortedDictionary(e => e.Name, e => e);

        private static readonly ImmutableList<Condition> DefenseConditions = new Condition[]
        {
            new DefensePenalty(DefenseType.ArmorClass),
            new DefensePenalty(DefenseType.Fortitude),
            new DefensePenalty(DefenseType.Reflex),
            new DefensePenalty(DefenseType.Will),
        }.ToImmutableList();

        public IEnumerable<IEffectModifier> GetBaseModifiers(UpgradeStage stage, TargetEffect target, AttackProfile? attack, PowerProfileBuilder power)
        {
            return new ConditionModifier(ImmutableList<Condition>.Empty).GetUpgrades(stage, target, attack, power);
        }

        public static IEnumerable<Condition> GetBaseConditions()
        {
            return from set in new[]
                   {
                       from basicCondition in basicConditions.Keys
                       where basicConditions[basicCondition].AllowDirectApplication
                       select new Condition(basicCondition),
                       DefenseConditions,
                   }
                   from mod in set
                   select mod;
        }

        public static double DurationMultiplier(Duration duration) =>
            duration == Duration.EndOfEncounter ? 4
            : duration == Duration.SaveEnds ? 2 // Must remain "SaveEnds" if there's a Boost dependent upon it
            : 1;

        public IEnumerable<IPowerModifier> GetBaseModifiers(UpgradeStage stage, PowerProfileBuilder power)
        {
            if (power.HasDuration())
                yield break;
            foreach (var entry in from duration in new[] { Duration.SaveEnds, Duration.EndOfEncounter }
                                  let p = power.Apply(new EffectDurationFormula.EffectDurationModifier(duration))
                                  from condition in new ConditionModifier(ImmutableList<Condition>.Empty).GetUpgrades(stage, p.Attacks[0].Effects[0], p.Attacks[0], p)
                                  select new EffectAndDurationModifier(duration, condition))
            {
                yield return entry;
            }
        }

        public record EffectAndDurationModifier(Duration Duration, IEffectModifier EffectModifier) : PowerModifier(ModifierName)
        {
            public override int GetComplexity(PowerHighLevelInfo powerInfo) => 1;
            public override PowerCost GetCost(PowerProfileBuilder builder) => PowerCost.Empty;

            public override PowerTextMutator? GetTextMutator(PowerProfile power)
            {
                throw new NotSupportedException("Should be removed before here");
            }

            public override IEnumerable<IPowerModifier> GetUpgrades(UpgradeStage stage, PowerProfileBuilder power)
            {
                yield break;
            }

            public override IEnumerable<PowerProfileBuilder> TrySimplifySelf(PowerProfileBuilder builder)
            {
                var next = builder with { Modifiers = builder.Modifiers.Remove(this).Add(new EffectDurationFormula.EffectDurationModifier(Duration)) };
                var newEffect = new TargetEffect(new BasicTarget(Target.Enemy), EffectType.Harmful, ImmutableList<IEffectModifier>.Empty.Add(EffectModifier));

                return from lens in (
                            from attackIndex in Enumerable.Range(0, next.Attacks.Count)
                            select attacksLens.To(attackIndex).To(attackToEffectLens)
                       ).Add(effectsLens)
                       from entry in (
                            from effectIndex in Enumerable.Range(0, next.Get(lens).Count)
                            let effectLens = lens.To(effectIndex)
                            where next.Get(effectLens).EffectType == EffectType.Harmful
                            select next.Update(effectLens, AddEffectModifier)
                       ).DefaultIfEmpty(next.Update(lens, effects => effects.Add(newEffect)))
                       select entry;

                TargetEffect AddEffectModifier(TargetEffect e) =>
                    e with { Modifiers = e.Modifiers.Items.Add(EffectModifier) };
            }
        }

        public record AfterEffect(Condition Condition, bool AfterFailedSave);

        public record ConditionModifier(EquatableImmutableList<Condition> Conditions, AfterEffect? AfterEffect = null) : EffectModifier(ModifierName)
        {
            public override int GetComplexity(PowerHighLevelInfo powerInfo) => (Conditions.Count + 2) / 3;
            public override PowerCost GetCost(TargetEffect builder, PowerProfileBuilder power) =>
                new PowerCost(Fixed:
                    AfterEffect switch
                    {
                        null => Conditions.Select(c => c.Cost() * DurationMultiplier(power.GetDuration())).Sum(),
                        { Condition: var afterEffect, AfterFailedSave: false } =>
                            Conditions.Select(c => c.Cost() * DurationMultiplier(Duration.SaveEnds)).Sum()
                            + afterEffect.Cost() * DurationMultiplier(Duration.SaveEnds),
                        { Condition: var afterEffect, AfterFailedSave: true } =>
                            Conditions.Select(c => c.Cost() * DurationMultiplier(Duration.EndOfUserNextTurn)).Sum()
                            + afterEffect.Cost() * DurationMultiplier(Duration.SaveEnds) / 2
                    });
            public override bool IsPlaceholder() => Conditions.Count == 0;
            public override bool UsesDuration() => Conditions.Any();
            public override bool IsInstantaneous() => false;
            public override bool IsBeneficial() => false;
            public override bool IsHarmful() => true;

            public override IEnumerable<IEffectModifier> GetUpgrades(UpgradeStage stage, TargetEffect target, AttackProfile? attack, PowerProfileBuilder power)
            {
                if (stage < UpgradeStage.Standard)
                    return Enumerable.Empty<IEffectModifier>();
                if (target.EffectType != EffectType.Harmful)
                    return Enumerable.Empty<IEffectModifier>();
                if (AfterEffect != null)
                    return Enumerable.Empty<IEffectModifier>();
                return from set in new[]
                {
                    from basicCondition in basicConditions.Keys
                    where Conditions.Count == 0 && power.GetDuration() == Duration.SaveEnds
                    where basicConditions[basicCondition].Subsumes.Count > 0
                    from simple in basicConditions[basicCondition].Subsumes
                    where basicConditions[simple].AllowDirectApplication
                    select new ConditionModifier(ImmutableList<Condition>.Empty.Add(new Condition(simple)), new AfterEffect(new Condition(basicCondition), true)),

                    from basicCondition in basicConditions.Keys
                    where Conditions.Count == 0 && power.GetDuration() == Duration.SaveEnds
                    where basicConditions[basicCondition].Subsumes.Count > 0
                    from simple in basicConditions[basicCondition].Subsumes
                    where basicConditions[simple].AllowDirectApplication
                    select new ConditionModifier(ImmutableList<Condition>.Empty.Add(new Condition(basicCondition)), new AfterEffect(new Condition(simple), false)),

                    from basicCondition in basicConditions.Keys
                    where basicConditions[basicCondition].AllowDirectApplication
                    where !Conditions.Select(b => b.Name).Contains(basicCondition)
                    where !GetSubsumed(Conditions).Contains(basicCondition)
                    select this with { Conditions = Filter(Conditions.Items.Add(new Condition(basicCondition))) },

                    from basicCondition in basicConditions.Keys
                    where !Conditions.Select(b => b.Name).Contains(basicCondition)
                    let newCondition = new Condition(basicCondition)
                    let filtered = Filter(Conditions.Items.Add(newCondition))
                    where filtered.Count == 1 && filtered[0].Name == basicCondition // the new condition subsumes all other conditions
                    select this with { AfterEffect = new (newCondition, true) },

                    from basicCondition in basicConditions.Keys
                    where !Conditions.Select(b => b.Name).Contains(basicCondition)
                    where GetSubsumed(Conditions).Contains(basicCondition)
                    let newCondition = new Condition(basicCondition) // the new condition is a lesser version of one of the others already applied
                    select this with { AfterEffect = new (newCondition, false) },

                    from defenseCondition in DefenseConditions
                    where !Conditions.OfType<DefensePenalty>().Any() // only allow one defense penalty
                    select this with { Conditions = Conditions.Items.Add(defenseCondition) },

                    // TODO - add OngoingDamage

                    from condition in Conditions
                    from upgrade in condition.GetUpgrades(power.PowerInfo)
                    select this with { Conditions = Filter(Conditions.Items.Remove(condition).Add(upgrade)) },
                }
                       from mod in set
                       select mod;
            }

            public override TargetInfoMutator? GetTargetInfoMutator(TargetEffect effect, PowerProfile power) =>
                new(0, (target) => target with
                {
                    Parts = target.Parts.AddRange(GetParts(effect, power)),
                    AdditionalSentences = target.AdditionalSentences.AddRange(GetAdditionalSentences(effect, power))
                });


            public IEnumerable<string> GetParts(TargetEffect effect, PowerProfile power)
            {
                var duration = power.GetDuration() switch
                {
                    Duration.EndOfUserNextTurn => "until the end of your next turn",
                    Duration.SaveEnds => "(save ends)",
                    Duration.EndOfEncounter => "until the end of the encounter",
                    _ => throw new System.NotImplementedException(),
                };

                yield return @$"{OxfordComma((from condition in Conditions
                                              group condition.Effect().ToLower() by condition.Verb(effect.Target.GetTarget()) into verbGroup
                                              select verbGroup.Key + " " + OxfordComma(verbGroup.ToArray())).ToArray())} {duration}";
            }


            public IEnumerable<string> GetAdditionalSentences(TargetEffect effect, PowerProfile power)
            {
                switch (AfterEffect)
                {
                    case { AfterFailedSave: true, Condition: var condition }:
                        yield return $"If the target fails its first saving throw against this power, the target {condition.Verb(effect.Target.GetTarget())} {condition.Effect().ToLower()} (save ends).";
                        break;
                    case { AfterFailedSave: false, Condition: var condition }:
                        // 4e used an "Aftereffect" entry, but this makes equal sense
                        yield return $"When the target succeeds against its saving throw against this power, the target {condition.Verb(effect.Target.GetTarget())} {condition.Effect().ToLower()} (save ends).";
                        break;
                }
            }

        }

        private static ImmutableList<Condition> Filter(ImmutableList<Condition> conditions)
        {
            var subsumed = GetSubsumed(conditions);
            return conditions.Where(c => !subsumed.Contains(c.Name)).ToImmutableList();
        }

        private static HashSet<string> GetSubsumed(ImmutableList<Condition> conditions) =>
            conditions.Select(c => c.Name).SelectMany(c => basicConditions.ContainsKey(c) && basicConditions[c].Subsumes.Any()
                                ? basicConditions[c].Subsumes
                                : Enumerable.Empty<string>()).ToHashSet();

        public record Condition(string Name)
        {
            public virtual double Cost() => basicConditions[Name].Cost;
            public virtual IEnumerable<Condition> GetUpgrades(PowerHighLevelInfo powerInfo) =>
                Enumerable.Empty<Condition>();
            public virtual string Verb(Target target) => target == Target.Self ? basicConditions[Name].SelfVerb : basicConditions[Name].OtherVerb;
            public virtual string Effect() => basicConditions[Name].Effect;
        }

        public record OngoingDamage(int Amount) : Condition("Ongoing")
        {
            public override double Cost() => Amount / 5;

            public override IEnumerable<Condition> GetUpgrades(PowerHighLevelInfo powerInfo)
            {
                if (Amount < 15)
                    yield return new OngoingDamage(Amount + 5);
            }
            public override string Verb(Target target) => target == Target.Self ? "take" : "takes";
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
            public override string Verb(Target target) => target == Target.Self ? "take" : "takes";
            public override string Effect() => $"a -2 penalty to {Defense?.ToText() switch { string s => "its " + s, _ => "all defenses" }}";
        }
    }
}
