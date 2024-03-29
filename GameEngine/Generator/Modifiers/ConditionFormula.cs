﻿using System.Collections.Generic;
using System.Collections.Immutable;
using GameEngine.Rules;
using System.Linq;
using static System.Collections.Immutable.ImmutableList<string>;
using System;
using static GameEngine.Generator.ProseHelpers;
using GameEngine.Generator.Text;
using GameEngine.Generator.Context;
using GameEngine.Combining;

namespace GameEngine.Generator.Modifiers
{
    public record ConditionFormula() : IEffectFormula, IPowerModifierFormula
    {
        private static readonly Lens<PowerProfile, ImmutableList<AttackProfile>> attacksLens = Lens<PowerProfile>.To(p => p.Attacks.Items, (p, a) => p with { Attacks = a });
        private static readonly Lens<PowerProfile, ImmutableList<TargetEffect>> effectsLens = Lens<PowerProfile>.To(p => p.Effects.Items, (p, e) => p with { Effects = e });
        private static readonly Lens<AttackProfile, ImmutableList<TargetEffect>> attackToEffectLens = Lens<AttackProfile>.To(p => p.Effects.Items, (p, e) => p with { Effects = e });

        public record ConditionOptionKey(BasicCondition Condition, Duration Duration);
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
                    AllowDirectApplication: false, AllowAfterEfect: true, Cost: 2,
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
                    AllowDirectApplication: false, AllowAfterEfect: false, Cost: 1.5,
                    OtherVerb: "is", SelfVerb: "are", Effect: "Helpless"
                ),
                new (Name: "Immobilized",
                    Subsumes: Empty.AddRange(new[] { "Slowed" }),
                    AllowDirectApplication: false, AllowAfterEfect: true, Cost: 1,
                    OtherVerb: "is", SelfVerb: "are", Effect: "Immobilized"
                ),
                new (Name: "Marked",
                    AllowDirectApplication: true, AllowAfterEfect: false, Cost: 0.5,
                    OtherVerb: "is", SelfVerb: "are", Effect: "Marked"
                ),
                new (Name: "Petrified",
                    Subsumes: Empty.AddRange(new[] { "Immobilized", "Slowed" }),
                    AllowDirectApplication: false, AllowAfterEfect: true, Cost: 2,
                    OtherVerb: "is", SelfVerb: "are", Effect: "Petrified"
                ),
                new (Name: "Restrained",
                    Subsumes: Empty.AddRange(new[] { "Immobilized", "Slowed" }),
                    AllowDirectApplication: true, AllowAfterEfect: false, Cost: 1.5,
                    OtherVerb: "is", SelfVerb: "are", Effect: "Restrained"
                ),
                new (Name: "Slowed",
                    AllowDirectApplication: true, AllowAfterEfect: true, Cost: 0.5,
                    OtherVerb: "is", SelfVerb: "are", Effect: "Slowed"
                ),
                new (Name: "Stunned",
                    Subsumes: Empty.AddRange(new[] { "Dazed", "Marked", "Surprised", "Grants Combat Advantage" }),
                    AllowDirectApplication: true, AllowAfterEfect: true, Cost: 1.5,
                    OtherVerb: "is", SelfVerb: "are", Effect: "Stunned"
                ),
                new (Name: "Surprised",
                    Subsumes: Empty.AddRange(new[] { "Dazed", "Grants Combat Advantage" }),
                    AllowDirectApplication: false, AllowAfterEfect: false, Cost: 1,
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

        public IEnumerable<IEffectModifier> GetBaseModifiers(UpgradeStage stage, EffectContext effectContext)
        {
            return new ConditionModifier(ImmutableList<Condition>.Empty).GetUpgrades(stage, effectContext);
        }

        public static IEnumerable<Condition> GetBaseConditions()
        {
            return from set in new []
                   {
                       from basicCondition in basicConditions.Keys
                       where basicConditions[basicCondition].AllowDirectApplication
                       select (Condition)new BasicCondition(basicCondition),
                       DefenseConditions,
                   }
                   from mod in set
                   select mod;
        }

        public static double DurationMultiplier(Duration duration) =>
            duration switch
            {
                Duration.EndOfEncounter => 4,
                Duration.StanceEnds => 2,
                Duration.SaveEnds => 2, // Must remain "SaveEnds" if there's a Boost dependent upon it
                _ => 1
            };

        public IEnumerable<IPowerModifier> GetBaseModifiers(UpgradeStage stage, PowerContext powerContext)
        {
            if (powerContext.HasDuration())
                yield break;
            if (!powerContext.Attacks.Any())
                yield break;
            foreach (var entry in from duration in new[] { Duration.SaveEnds, Duration.EndOfEncounter }
                                  from condition in new ConditionModifier(ImmutableList<Condition>.Empty).GetUpgrades(stage, duration)
                                  select new EffectAndDurationModifier(duration, condition))
            {
                yield return entry;
            }
        }

        public record EffectAndDurationModifier(Duration Duration, IEffectModifier EffectModifier) : RewritePowerModifier()
        {
            public override IEnumerable<PowerProfile> TrySimplifySelf(PowerProfile builder)
            {
                var next = builder with { Modifiers = builder.Modifiers.Items.Remove(this).Add(new EffectDurationFormula.EffectDurationModifier(Duration)) };
                var newEffect = new TargetEffect(new SameAsOtherTarget(0), EffectType.Harmful, ImmutableList<IEffectModifier>.Empty.Add(EffectModifier));

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

        [ModifierName("Condition")]
        public record ConditionModifier(EquatableImmutableList<Condition> Conditions, AfterEffect? AfterEffect = null) : EffectModifier()
        {
            public override int GetComplexity(PowerContext powerContext) => (Conditions.Count + 2) / 3;
            public override PowerCost GetCost(EffectContext effectContext) =>
                new PowerCost(Fixed:
                    AfterEffect switch
                    {
                        null => Conditions.Select(c => c.Cost() * DurationMultiplier(effectContext.GetDuration())).Sum(),
                        { Condition: var afterEffect, AfterFailedSave: false } =>
                            Conditions.Select(c => c.Cost() * DurationMultiplier(Duration.SaveEnds)).Sum()
                            + afterEffect.Cost() * DurationMultiplier(Duration.SaveEnds),
                        { Condition: var afterEffect, AfterFailedSave: true } =>
                            Conditions.Select(c => c.Cost() * DurationMultiplier(Duration.EndOfUserNextTurn)).Sum()
                            + afterEffect.Cost() * DurationMultiplier(Duration.SaveEnds) / 2
                    });
            public override ModifierFinalizer<IEffectModifier>? Finalize(EffectContext powerContext) =>
                Conditions.Count == 0
                    ? () => null
                    : null;
            public override bool UsesDuration() => Conditions.Any();
            public override bool IsInstantaneous() => false;
            public override bool IsBeneficial() => false;
            public override bool IsHarmful() => true;

            public IEnumerable<IEffectModifier> GetUpgrades(UpgradeStage stage, Duration duration)
            {

                if (stage < UpgradeStage.Standard)
                    return Enumerable.Empty<IEffectModifier>();
                if (AfterEffect != null)
                    return Enumerable.Empty<IEffectModifier>();
                return from set in new[]
                {
                    from basicCondition in basicConditions.Keys
                    where Conditions.Count == 0 && duration == Duration.SaveEnds
                    where basicConditions[basicCondition].Subsumes.Count > 0
                    from simple in basicConditions[basicCondition].Subsumes
                    where basicConditions[simple].AllowDirectApplication
                    select new ConditionModifier(ImmutableList<Condition>.Empty.Add(new BasicCondition(simple)), new AfterEffect(new BasicCondition(basicCondition), true)),

                    from basicCondition in basicConditions.Keys
                    where Conditions.Count == 0 && duration == Duration.SaveEnds
                    where basicConditions[basicCondition].Subsumes.Count > 0
                    from simple in basicConditions[basicCondition].Subsumes
                    where basicConditions[simple].AllowDirectApplication
                    select new ConditionModifier(ImmutableList<Condition>.Empty.Add(new BasicCondition(basicCondition)), new AfterEffect(new BasicCondition(simple), false)),

                    from basicCondition in basicConditions.Keys
                    where basicConditions[basicCondition].AllowDirectApplication
                    where !Conditions.OfType<BasicCondition>().Select(b => b.ConditionName).Contains(basicCondition)
                    where !GetSubsumed(Conditions).Contains(basicCondition)
                    select this with { Conditions = Filter(Conditions.Items.Add(new BasicCondition(basicCondition))) },

                    from basicCondition in basicConditions.Keys
                    where !Conditions.OfType<BasicCondition>().Select(b => b.ConditionName).Contains(basicCondition) && Conditions.Count > 0
                    where duration == Duration.SaveEnds
                    let newCondition = new BasicCondition(basicCondition)
                    let filtered = Filter(Conditions.Items.Add(newCondition))
                    where filtered.Count == 1 && filtered[0] is BasicCondition b && b.ConditionName == basicCondition // the new condition subsumes all other conditions
                    select this with { AfterEffect = new (newCondition, true) },

                    from basicCondition in basicConditions.Keys
                    where duration == Duration.SaveEnds
                    where Conditions.Count > 0
                    where !Conditions.OfType<BasicCondition>().Select(b => b.ConditionName).Contains(basicCondition)
                    where GetSubsumed(Conditions).Contains(basicCondition)
                    let newCondition = new BasicCondition(basicCondition) // the new condition is a lesser version of one of the others already applied
                    select this with { AfterEffect = new (newCondition, false) },

                    from amount in Enumerable.Repeat(5, 1)
                    where !Conditions.OfType<OngoingDamage>().Any()
                    select this with { Conditions = Conditions.Items.Add(new OngoingDamage(amount)) },

                    from defenseCondition in DefenseConditions
                    where !Conditions.OfType<DefensePenalty>().Any() // only allow one defense penalty
                    select this with { Conditions = Conditions.Items.Add(defenseCondition) },

                    from condition in Conditions
                    from upgrade in condition.GetUpgrades()
                    select this with { Conditions = Filter(Conditions.Items.Remove(condition).Add(upgrade)) },
                }
                       from mod in set
                       select mod;
            }

            public override IEnumerable<IEffectModifier> GetUpgrades(UpgradeStage stage, EffectContext effectContext)
            {
                if (effectContext.EffectType != EffectType.Harmful)
                    return Enumerable.Empty<IEffectModifier>();
                return GetUpgrades(stage, effectContext.GetDuration());
            }

            public override TargetInfoMutator? GetTargetInfoMutator(EffectContext effectContext, bool half)
            {
                if (half)
                {
                    var (targetMod, duration) = Half(this, effectContext.GetDuration());
                    return MutatorFor(effectContext, targetMod, duration);
                }
                return MutatorFor(effectContext, this, effectContext.GetDuration());
            }

            private static TargetInfoMutator MutatorFor(EffectContext effectContext, ConditionModifier conditionModifier, Duration duration)
            {
                return new(0, (target) => target with
                {
                    Parts = target.Parts.AddRange(conditionModifier.GetParts(effectContext, duration)),
                    AdditionalSentences = target.AdditionalSentences.AddRange(conditionModifier.GetAdditionalSentences(effectContext))
                });
            }

            private static (ConditionModifier, Duration) Half(ConditionModifier conditionModifier, Duration duration)
            {
                if (conditionModifier.AfterEffect != null) return (conditionModifier with { AfterEffect = null }, duration);
                if (duration is not Duration.StanceEnds and not Duration.EndOfUserNextTurn) return (conditionModifier, duration - 1);
                return (
                    conditionModifier with
                    {
                        Conditions = conditionModifier.Conditions.OrderByDescending(c => c.Cost()).Skip(1).ToImmutableList()
                    }, 
                    duration
                );
            }

            public IEnumerable<string> GetParts(EffectContext effectContext, Duration duration)
            {
                var durationText = duration switch
                {
                    Duration.EndOfUserNextTurn => "until the end of your next turn",
                    Duration.SaveEnds => "(save ends)",
                    Duration.EndOfEncounter => "until the end of the encounter",
                    Duration.StanceEnds => "until the stance ends",
                    _ => throw new System.NotImplementedException(),
                };

                if (!Conditions.Any())
                    yield break;

                yield return @$"{OxfordComma((from condition in Conditions
                                              group condition.Effect().ToLower() by condition.Verb(effectContext.Target) into verbGroup
                                              select verbGroup.Key + " " + OxfordComma(verbGroup.ToArray())).ToArray())} {durationText}";
            }


            public IEnumerable<string> GetAdditionalSentences(EffectContext effectContext)
            {
                switch (AfterEffect)
                {
                    case { AfterFailedSave: true, Condition: var condition }:
                        yield return $"If the target fails its first saving throw against this power, the target {condition.Verb(effectContext.Target)} {condition.Effect().ToLower()} (save ends).";
                        break;
                    case { AfterFailedSave: false, Condition: var condition }:
                        // 4e used an "Aftereffect" entry, but this makes equal sense
                        yield return $"When the target succeeds against its saving throw against this power, the target {condition.Verb(effectContext.Target)} {condition.Effect().ToLower()} (save ends).";
                        break;
                }
            }


            public override CombineResult<IEffectModifier> Combine(IEffectModifier mod)
            {
                if (mod is not ConditionModifier other)
                    return CombineResult<IEffectModifier>.Cannot;

                return new CombineResult<IEffectModifier>.CombineToOne(
                    new ConditionModifier(
                        (from condition in Conditions.Concat(other.Conditions)
                         group condition by condition.GetType() into conditionsByType
                         from condition in CombineConditions(conditionsByType)
                         select condition).ToImmutableList()
                    )
                );

                IEnumerable<Condition> CombineConditions(IEnumerable<Condition> conditions)
                {
                    if (!conditions.Any(c => c is BasicCondition))
                        return conditions.OrderByDescending(c => c.Cost()).Take(1);
                    return Filter(conditions.Distinct().ToImmutableList());
                }
            }
        }

        private static ImmutableList<Condition> Filter(ImmutableList<Condition> conditions)
        {
            var subsumed = GetSubsumed(conditions);
            return conditions.Where(c => c is not BasicCondition basic || !subsumed.Contains(basic.ConditionName)).ToImmutableList();
        }

        private static HashSet<string> GetSubsumed(ImmutableList<Condition> conditions) =>
            conditions
                .OfType<BasicCondition>()
                .Select(c => c.ConditionName)
                .SelectMany(c => 
                    basicConditions.ContainsKey(c) && basicConditions[c].Subsumes.Any()
                        ? basicConditions[c].Subsumes
                        : Enumerable.Empty<string>()
                )
                .ToHashSet();

        public abstract record Condition()
        {
            public abstract double Cost();
            public abstract IEnumerable<Condition> GetUpgrades();
            public abstract string Verb(Target target);
            public abstract string Effect();
        }

        [ModifierName("Simple")]
        public record BasicCondition(string ConditionName) : Condition()
        {
            public override double Cost() => basicConditions[ConditionName].Cost;
            public override IEnumerable<Condition> GetUpgrades() =>
                Enumerable.Empty<Condition>();
            public override string Verb(Target target) => target == Target.Self ? basicConditions[ConditionName].SelfVerb : basicConditions[ConditionName].OtherVerb;
            public override string Effect() => basicConditions[ConditionName].Effect;
        }

        [ModifierName("Ongoing")]
        public record OngoingDamage(int Amount) : Condition()
        {
            public override double Cost() => Amount / 5;

            public override IEnumerable<Condition> GetUpgrades()
            {
                if (Amount < 15)
                    yield return new OngoingDamage(Amount + 5);
            }
            public override string Verb(Target target) => target == Target.Self ? "take" : "takes";
            public override string Effect() => $"ongoing {Amount}";
        }

        [ModifierName("Defense Penalty")]
        public record DefensePenalty(DefenseType? Defense) : Condition()
        {
            public override double Cost() => Defense == null ? 1 : 0.5;

            public override IEnumerable<Condition> GetUpgrades()
            {
                if (Defense != null)
                    yield return new DefensePenalty((DefenseType?)null);
            }
            public override string Verb(Target target) => target == Target.Self ? "take" : "takes";
            public override string Effect() => $"a -2 penalty to {Defense?.ToText() switch { string s => "its " + s, _ => "all defenses" }}";
        }
    }
}
