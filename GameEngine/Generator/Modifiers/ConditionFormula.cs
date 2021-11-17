using System.Collections.Generic;
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

        public record ConditionDefinition(string Name, double Cost, string OtherVerb, string SelfVerb, string Effect, bool AllowDirectApplication, ImmutableList<string> Subsumes)
        {
            public ConditionDefinition(string Name, double Cost, string OtherVerb, string SelfVerb, string Effect, bool AllowDirectApplication)
                : this(Name: Name, Cost: Cost, OtherVerb: OtherVerb, SelfVerb: SelfVerb, Effect: Effect, AllowDirectApplication: AllowDirectApplication, Subsumes: Empty)
            {
            }
        }

        private static readonly ImmutableSortedDictionary<string, ConditionDefinition> basicConditions =
            new ConditionDefinition[]
            {
                new (Name: "Blinded", 
                    Subsumes: Empty.Add("Grants Combat Advantage"),
                    AllowDirectApplication: true, Cost: 1, 
                    OtherVerb: "is", SelfVerb: "are", Effect: "Blinded"
                ),
                new (Name: "Dazed", 
                    Subsumes: Empty.Add("Grants Combat Advantage"),
                    AllowDirectApplication: true, Cost: 0.5, 
                    OtherVerb: "is", SelfVerb: "are", Effect: "Dazed"
                ),
                new (Name: "Deafened", 
                    AllowDirectApplication: true, Cost: 0.5, 
                    OtherVerb: "is", SelfVerb: "are", Effect: "Deafened"
                ),
                new (Name: "Dominated", 
                    Subsumes: Empty.AddRange(new[] { "Dazed", "Grants Combat Advantage" }),
                    AllowDirectApplication: false, Cost: 2, // TODO - check cost
                    OtherVerb: "is", SelfVerb: "are", Effect: "Dominated"
                ),
                new (Name: "Dying", 
                    Subsumes: Empty.AddRange(new[] { "Unconscious", "Immobilized", "Slowed", "Weakened", "Blinded", "Deafened", "Stunned", "Dazed", "Marked", "Surprised", "Grants Combat Advantage" }),
                    AllowDirectApplication: false, Cost: 4, 
                    OtherVerb: "is", SelfVerb: "are", Effect: "Dying"
                ),
                new (Name: "Grants Combat Advantage", 
                    AllowDirectApplication: true, Cost: 0.5, 
                    OtherVerb: "grants", SelfVerb: "grant", Effect: "Combat Advantage"
                ),
                new (Name: "Helpless", 
                    Subsumes: Empty.AddRange(new[] { "Grants Combat Advantage" }),
                    AllowDirectApplication: false, Cost: 1.5, // TODO - check cost
                    OtherVerb: "is", SelfVerb: "are", Effect: "Helpless"
                ),
                new (Name: "Immobilized", 
                    Subsumes: Empty.AddRange(new[] { "Slowed" }),
                    AllowDirectApplication: false, Cost: 1, 
                    OtherVerb: "is", SelfVerb: "are", Effect: "Immobilized"
                ),
                new (Name: "Marked", 
                    AllowDirectApplication: true, Cost: 0.5, // TODO - check cost
                    OtherVerb: "is", SelfVerb: "are", Effect: "Marked"
                ),
                new (Name: "Petrified", 
                    Subsumes: Empty.AddRange(new[] { "Immobilized", "Slowed" }),
                    AllowDirectApplication: false, Cost: 2, // TODO - check cost
                    OtherVerb: "is", SelfVerb: "are", Effect: "Petrified"
                ),
                new (Name: "Restrained", 
                    Subsumes: Empty.AddRange(new[] { "Immobilized", "Slowed" }),
                    AllowDirectApplication: true, Cost: 2, // TODO - check cost
                    OtherVerb: "is", SelfVerb: "are", Effect: "Restrained"
                ),
                new (Name: "Slowed", 
                    AllowDirectApplication: true, Cost: 0.5, 
                    OtherVerb: "is", SelfVerb: "are", Effect: "Slowed"
                ),
                new (Name: "Stunned", 
                    Subsumes: Empty.AddRange(new[] { "Dazed", "Marked", "Surprised", "Grants Combat Advantage" }),
                    AllowDirectApplication: true, Cost: 1, // TODO - check cost
                    OtherVerb: "is", SelfVerb: "are", Effect: "Stunned"
                ),
                new (Name: "Surprised", 
                    Subsumes: Empty.AddRange(new[] { "Dazed", "Grants Combat Advantage" }),
                    AllowDirectApplication: true, Cost: 1, // TODO - check cost
                    OtherVerb: "is", SelfVerb: "are", Effect: "Surprised"
                ),
                new (Name: "Unconscious", 
                    Subsumes: Empty.AddRange(new[] { "Immobilized", "Slowed", "Weakened", "Blinded", "Deafened", "Stunned", "Dazed", "Marked", "Surprised", "Grants Combat Advantage" }),
                    AllowDirectApplication: false, Cost: 2, 
                    OtherVerb: "becomes", SelfVerb: "become", Effect: "Unconscious"
                ),
                new (Name: "Weakened", 
                    AllowDirectApplication: true, Cost: 1, 
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
            foreach (var entry in from condition in GetBaseConditions()
                                  from duration in new[] { Duration.SaveEnds, Duration.EndOfEncounter }
                                  select new EffectAndDurationModifier(duration, new ConditionModifier(ImmutableList<Condition>.Empty.Add(condition))))
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

        public record ConditionModifier(EquatableImmutableList<Condition> Conditions) : EffectModifier(ModifierName)
        {
            public override int GetComplexity(PowerHighLevelInfo powerInfo) => (Conditions.Count + 2) / 3;
            public override PowerCost GetCost(TargetEffect builder, PowerProfileBuilder power) =>
                new PowerCost(Fixed: Conditions.Select(c => c.Cost() * DurationMultiplier(power.GetDuration())).Sum());
            public override bool IsPlaceholder() => Conditions.Count == 0;
            public override bool UsesDuration() => Conditions.Any();
            public override bool IsInstantaneous() => false;
            public override bool IsBeneficial() => false;
            public override bool IsHarmful() => true;

            public override IEnumerable<IEffectModifier> GetUpgrades(UpgradeStage stage, TargetEffect target, AttackProfile? attack, PowerProfileBuilder power) =>
                (stage < UpgradeStage.Standard) || target.EffectType != EffectType.Harmful
                    ? Enumerable.Empty<IEffectModifier>()
                    : from set in new[]
                      {
                          from basicCondition in basicConditions.Keys
                          where basicConditions[basicCondition].AllowDirectApplication
                          where !Conditions.Select(b => b.Name).Contains(basicCondition)
                          where !GetSubsumed(Conditions).Contains(basicCondition)
                          select this with { Conditions = Filter(Conditions.Items.Add(new Condition(basicCondition))) },

                          from defenseCondition in DefenseConditions
                          where !Conditions.OfType<DefensePenalty>().Any() // only allow one defense penalty
                          select this with { Conditions = Conditions.Items.Add(defenseCondition) },

                          from condition in Conditions
                          from upgrade in condition.GetUpgrades(power.PowerInfo)
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
                conditions.Select(c => c.Name).SelectMany(c => basicConditions.ContainsKey(c) && basicConditions[c].Subsumes.Any()
                                    ? basicConditions[c].Subsumes
                                    : Enumerable.Empty<string>()).ToHashSet();

            public override TargetInfoMutator? GetTargetInfoMutator(TargetEffect effect, PowerProfile power) =>
                new(0, (target) => target with
                {
                    Parts = target.Parts.AddRange(GetParts(effect, power)),
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

                var parts = new List<string>();
                yield return @$"{OxfordComma((from condition in Conditions
                                              group condition.Effect().ToLower() by condition.Verb(effect.Target.GetTarget()) into verbGroup
                                              select verbGroup.Key + " " + OxfordComma(verbGroup.ToArray())).ToArray())} {duration}";
            }

        }

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
