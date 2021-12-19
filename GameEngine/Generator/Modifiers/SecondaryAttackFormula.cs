using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using GameEngine.Combining;
using GameEngine.Generator.Context;
using GameEngine.Generator.Text;
using GameEngine.Rules;
using static GameEngine.Generator.ImmutableConstructorExtension;
using static GameEngine.Generator.ProseHelpers;

namespace GameEngine.Generator.Modifiers
{
    public record MultiattackFormula() : IPowerModifierFormula
    {
        public IEnumerable<IPowerModifier> GetBaseModifiers(UpgradeStage stage, PowerContext powerContext)
        {
            if (stage != UpgradeStage.Standard)
                yield break;
            if (powerContext.Modifiers.OfType<IUniquePowerModifier>().Any())
                yield break;
            if (powerContext.Attacks.Count != 1)
                yield break;

            yield return new TargetDelegateModifier(new TwoHitsModifier());
            yield return new TargetDelegateModifier(new UpToThreeTargetsModifier());

            foreach (var count in new[] { 2, 3 })
            {
                yield return new SplitAttackModifier(count, false);
                yield return new SplitAttackModifier(count, true);
            }
        }

        public record TargetDelegateModifier(IAttackTargetModifier TargetModifier) : RewritePowerModifier()
        {
            public override IEnumerable<PowerProfile> TrySimplifySelf(PowerProfile power)
            {
                var attack = power.Attacks.Single();
                yield return power with
                {
                    Attacks = new[] {
                        attack with
                        {
                            Target = TargetModifier,
                            Effects = attack.Effects.Items.SetItem(0, attack.Effects[0] with { Target = new SameAsOtherTarget() }),
                        }
                    }.ToImmutableList(),
                    Modifiers = power.Modifiers.Items.Remove(this).Add(new MultiattackAppliedModifier()),
                };
            }
        }

        private static Lens<IModifier, int?> damageOrderLens = Lens<IModifier>.To(mod => ((DamageModifier)mod).Order, (mod, newWeight) => ((DamageModifier)mod) with { Order = newWeight });
        public record SplitAttackModifier(int AmountsCount, bool RequiresPreviousHit) : RewritePowerModifier()
        {
            public override IEnumerable<PowerProfile> TrySimplifySelf(PowerProfile power)
            {
                var attack = power.Attacks.Single();
                var resultPower = power with
                {
                    Attacks = Enumerable.Range(0, AmountsCount).Select((index) =>
                        attack with
                        {
                            Modifiers = index switch
                            {
                                0 when RequiresPreviousHit => Build<IAttackModifier>(new MustHitForNextAttackModifier()),
                                _ when index < AmountsCount - 1 && RequiresPreviousHit => Build<IAttackModifier>(new SecondaryAttackModifier(), new MustHitForNextAttackModifier()),
                                _ when RequiresPreviousHit => attack.Modifiers.Add(new SecondaryAttackModifier()).ToImmutableList(),

                                _ when index < AmountsCount - 1 && !RequiresPreviousHit => ImmutableList<IAttackModifier>.Empty,
                                _ => attack.Modifiers.ToImmutableList(),
                            }
                        }
                    ).ToImmutableList(),
                    Modifiers = power.Modifiers.Items.Remove(this).Add(new MultiattackAppliedModifier()),
                };
                var weights = Enumerable.Range(0, AmountsCount).Select(i => i * 0.5 + 0.5);
                var damageMods = (from lens in resultPower.GetModifierLenses()
                                  where resultPower.Get(lens) is DamageModifier
                                  select lens.To(damageOrderLens))
                    .Select((lens, index) => (lens, index))
                    .ToArray();
                yield return damageMods.Aggregate(resultPower, (currentPower, tuple) => currentPower.Replace(tuple.lens, tuple.index + 1));
                yield return damageMods.Aggregate(resultPower, (currentPower, tuple) => currentPower.Replace(tuple.lens, damageMods.Length - tuple.index));
            }
        }

        [ModifierName("Multiattack Applied")]
        public record MultiattackAppliedModifier() : PowerModifier(), IUniquePowerModifier
        {
            public override int GetComplexity(PowerContext powerContext) => 0;
            public override PowerCost GetCost(PowerContext powerContext) => PowerCost.Empty;
            public override ModifierFinalizer<IPowerModifier>? Finalize(PowerContext powerContext) => () => null;
            public override IEnumerable<IPowerModifier> GetUpgrades(UpgradeStage stage, PowerContext powerContext) => Enumerable.Empty<IPowerModifier>();
            public override PowerTextMutator? GetTextMutator(PowerContext powerContext) => throw new NotSupportedException("Should be upgraded or removed before this point");
        }

        [ModifierName("RequiredHitForNextAttack")]
        public record MustHitForNextAttackModifier() : AttackModifier()
        {
            public override int GetComplexity(PowerContext powerContext) => 0;

            public override PowerCost GetCost(AttackContext attackContext) => PowerCost.Empty;
            public override bool CanUseRemainingPower() => true;
            public override IEnumerable<IAttackModifier> GetUpgrades(UpgradeStage stage, AttackContext attackContext) =>
                Enumerable.Empty<IAttackModifier>();

            public override AttackInfoMutator? GetAttackInfoMutator(AttackContext attackContext) =>
                new(int.MaxValue, (attack, index) => attack with
                {
                    HitSentences = attack.HitSentences.Add($"Make a {Ordinal(attackContext.AttackIndex + 2)} attack."),
                });
        }

        [ModifierName("Multiattack")]
        public record SecondaryAttackModifier() : AttackModifier()
        {
            public const double FollowupAttackPower = 1.5;

            public override int GetComplexity(PowerContext powerContext) => 0;

            public const string ModifierName = "RequiresPreviousHit";

            public override PowerCost GetCost(AttackContext attackContext) =>
                new PowerCost(Multiplier: 1 / FollowupAttackPower);
            public override bool CanUseRemainingPower() => true;
            public override IEnumerable<IAttackModifier> GetUpgrades(UpgradeStage stage, AttackContext attackContext) =>
                Enumerable.Empty<IAttackModifier>();
            public override AttackInfoMutator? GetAttackInfoMutator(AttackContext attackContext) => null;
        }

        // Two Identical attacks
        [ModifierName("TwoHits")]
        public record TwoHitsModifier(EquatableImmutableList<IEffectModifier>? BothAttacksHitModifiers) : IAttackTargetModifier
        {
            public TwoHitsModifier() : this(ImmutableList<IEffectModifier>.Empty)
            {
            }

            public int GetComplexity(PowerContext powerContext) => Math.Max(1, BothAttacksHitModifiers.Select(a => a.GetComplexity(powerContext)).Sum());
            public PowerCost GetCost(AttackContext attackContext) =>
                new PowerCost(Multiplier: 2)
                + GetBothAttacksHitCost(attackContext) * 0.5;

            private PowerCost GetBothAttacksHitCost(AttackContext attackContext)
            {
                if (BothAttacksHitModifiers is not { Count: > 0 }) return PowerCost.Empty;

                var effectContext = SameAsOtherTarget.FindContextAt(attackContext);

                var orig = ModifiersCost(effectContext.Modifiers);
                var bothHitCost = ModifiersCost(effectContext.Modifiers.AddRange(BothAttacksHitModifiers).CombineList());
                return new PowerCost(bothHitCost.Fixed - orig.Fixed);

                PowerCost ModifiersCost(ImmutableList<IEffectModifier> modifiers) => modifiers.Select(m => m.GetCost(effectContext)).Sum();
            }

            public AttackType GetAttackType(AttackContext attackContext) =>
                (attackContext.ToolType, attackContext.ToolRange) switch
                {
                    (ToolType.Weapon, ToolRange.Melee) => new MeleeWeaponAttackType(),
                    (ToolType.Implement, ToolRange.Melee) => new MeleeTouchAttackType(),
                    (ToolType.Weapon, ToolRange.Range) => new RangedWeaponAttackType(),
                    (ToolType.Implement, ToolRange.Range) => new RangedAttackType(10),
                    _ => throw new NotSupportedException(),
                };

            public Target GetTarget(AttackContext attackContext) => Target.Ally | Target.Enemy;

            public string GetTargetText(AttackContext attackContext) => "one or two creatures";
            public string? GetAttackNotes(AttackContext attackContext) => "two attacks";
            public bool CanUseRemainingPower() => true;
            public TargetInfoMutator? GetTargetInfoMutator(AttackContext attackContext)
            {
                if (BothAttacksHitModifiers is not { Count: > 0 })
                    return null;
                var effectContext = SameAsOtherTarget.FindContextAt(attackContext);
                
                var bothAttacksHitTargetInfo = effectContext.GetTargetInfoForEffects(BothAttacksHitModifiers);

                return new TargetInfoMutator(100, (targetInfo) =>
                {
                    return targetInfo with
                    {
                        AdditionalRules = targetInfo.AdditionalRules
                            .Add(new ("Both Hit Same Target, Also", string.Join(" ",
                                ImmutableList<string>.Empty.Add(bothAttacksHitTargetInfo.PartsToSentence())
                                    .AddRange(bothAttacksHitTargetInfo.AdditionalSentences).Where(s => s is { Length: > 0 })
                            ))),
                    };
                });
            }

            public IEnumerable<IAttackTargetModifier> GetUpgrades(UpgradeStage stage, AttackContext attackContext)
            {
                if (stage != UpgradeStage.Standard)
                    return Enumerable.Empty<IAttackTargetModifier>();
                var effectContext = SameAsOtherTarget.FindContextAt(attackContext);

                return from set in new IEnumerable<IAttackTargetModifier>[]
                {
                    from formula in ModifierDefinitions.effectModifiers
                    from mod in formula.GetBaseModifiers(stage, effectContext)
                    where !BothAttacksHitModifiers.Any(m => m.Combine(mod) is CombineResult<IEffectModifier>.CombineToOne)
                        && !effectContext.Modifiers.Any(m => m.Combine(mod) is CombineResult<IEffectModifier>.CombineToOne { Result: var combined } && combined == m)
                    select this with { BothAttacksHitModifiers = (BothAttacksHitModifiers?.Items ?? ImmutableList<IEffectModifier>.Empty).Add(mod) },

                    from modifier in (BothAttacksHitModifiers?.Items ?? ImmutableList<IEffectModifier>.Empty)
                    from upgrade in modifier.GetUpgrades(stage, effectContext)
                    select this with { BothAttacksHitModifiers = (BothAttacksHitModifiers?.Items ?? ImmutableList<IEffectModifier>.Empty).Apply(upgrade, modifier) },

                    from modifier in effectContext.Modifiers
                    from upgrade in modifier.GetUpgrades(stage, effectContext)
                    where !BothAttacksHitModifiers.Any(m => m.Combine(upgrade) is CombineResult<IEffectModifier>.CombineToOne)
                    select this with { BothAttacksHitModifiers = (BothAttacksHitModifiers?.Items ?? ImmutableList<IEffectModifier>.Empty).Apply(upgrade, modifier) },
                }
                       from entry in set
                       select entry;
            }

            private static readonly Lens<TwoHitsModifier, ImmutableList<IEffectModifier>> innerEffectsLens = Lens<TwoHitsModifier>.To(m => (m.BothAttacksHitModifiers?.Items ?? ImmutableList<IEffectModifier>.Empty), (m, e) => m with { BothAttacksHitModifiers = e });
            public IEnumerable<Lens<IModifier, IModifier>> GetNestedModifiers()
            {
                return innerEffectsLens.EachItem(this).Select(lens => lens.CastInput<IModifier>().CastOutput<IModifier>()).ToArray();
            }

            public IAttackTargetModifier Finalize(AttackContext attackContext)
            {
                var effectContext = SameAsOtherTarget.FindContextAt(attackContext);
                return this.BothAttacksHitModifiers is { Count: > 0 } && GetBothAttacksHitCost(attackContext) is { Fixed: > 0 }
                    ? this with { BothAttacksHitModifiers = effectContext.Modifiers.AddRange(BothAttacksHitModifiers).RemoveRange(effectContext.Modifiers) }
                    : this with { BothAttacksHitModifiers = null };
            }
        }

        // Identical attacks against up to 3 targets.
        [ModifierName("UpToThreeTargets")]
        public record UpToThreeTargetsModifier() : IAttackTargetModifier
        {
            public int GetComplexity(PowerContext powerContext) => 0;
            public PowerCost GetCost(AttackContext attackContext) => new PowerCost(1.5);

            public AttackType GetAttackType(AttackContext attackContext) =>
                (attackContext.ToolType, attackContext.ToolRange) switch
                {
                    (ToolType.Weapon, ToolRange.Melee) => new MeleeWeaponAttackType(),
                    (ToolType.Implement, ToolRange.Melee) => new MeleeTouchAttackType(),
                    (ToolType.Weapon, ToolRange.Range) => new RangedWeaponAttackType(),
                    (ToolType.Implement, ToolRange.Range) => new RangedAttackType(10),
                    _ => throw new NotSupportedException(),
                };

            public Target GetTarget(AttackContext attackContext) => Target.Ally | Target.Enemy;

            public string GetTargetText(AttackContext attackContext) => "one, two, or three creatures";
            public string? GetAttackNotes(AttackContext attackContext) => "one attack per target";
            public bool CanUseRemainingPower() => true;
            public TargetInfoMutator? GetTargetInfoMutator(AttackContext attackContext) => null;

            public IEnumerable<IAttackTargetModifier> GetUpgrades(UpgradeStage stage, AttackContext attackContext) =>
                Enumerable.Empty<IAttackTargetModifier>();
            public IAttackTargetModifier Finalize(AttackContext powerContext) => this;
        }
    }
}
