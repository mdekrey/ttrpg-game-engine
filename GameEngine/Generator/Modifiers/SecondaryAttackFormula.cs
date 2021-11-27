using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using GameEngine.Generator.Context;
using GameEngine.Generator.Text;
using GameEngine.Rules;
using static GameEngine.Generator.ImmutableConstructorExtension;
using static GameEngine.Generator.PowerBuildingExtensions;
using static GameEngine.Generator.ProseHelpers;

namespace GameEngine.Generator.Modifiers
{
    public record MultiattackFormula() : IPowerModifierFormula
    {
        public const string ModifierName = "Multiattack";

        public IEnumerable<IPowerModifier> GetBaseModifiers(UpgradeStage stage, PowerContext powerContext)
        {
            if (powerContext.Modifiers.OfType<MultiattackAppliedModifier>().Any())
                yield break;
            if (stage != UpgradeStage.Standard)
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

        public record SplitAttackModifier(int AmountsCount, bool RequiresPreviousHit) : RewritePowerModifier()
        {
            public override IEnumerable<PowerProfile> TrySimplifySelf(PowerProfile power)
            {
                var attack = power.Attacks.Single();
                yield return power with
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
            }
        }

        public record MultiattackAppliedModifier() : PowerModifier(ModifierName)
        {
            public override int GetComplexity(PowerContext powerContext) => 0;
            public override PowerCost GetCost(PowerContext powerContext) => PowerCost.Empty;
            public override ModifierFinalizer<IPowerModifier>? Finalize(PowerContext powerContext) => () => null;
            public override IEnumerable<IPowerModifier> GetUpgrades(UpgradeStage stage, PowerContext powerContext) => Enumerable.Empty<IPowerModifier>();
            public override PowerTextMutator? GetTextMutator(PowerContext powerContext) => throw new NotSupportedException("Should be upgraded or removed before this point");
        }

        public record MustHitForNextAttackModifier() : AttackModifier(MustHitForNextAttackModifier.ModifierName)
        {
            public override int GetComplexity(PowerContext powerContext) => 0;

            public const string ModifierName = "RequiredHitForNextAttack";

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

        public record SecondaryAttackModifier() : AttackModifier(SecondaryAttackModifier.ModifierName)
        {
            public const double FollowupAttackPower = 1.5;

            public override int GetComplexity(PowerContext powerContext) => 0;

            public const string ModifierName = "RequiresPreviousHit";

            public override PowerCost GetCost(AttackContext attackContext) =>
                new PowerCost(Multiplier: 1 / FollowupAttackPower, SingleTargetMultiplier: 1 / FollowupAttackPower);
            public override bool CanUseRemainingPower() => true;
            public override IEnumerable<IAttackModifier> GetUpgrades(UpgradeStage stage, AttackContext attackContext) =>
                Enumerable.Empty<IAttackModifier>();
            public override AttackInfoMutator? GetAttackInfoMutator(AttackContext attackContext) => null;
        }

        // Two Identical attacks
        public record TwoHitsModifier(IEffectModifier? EffectModifier = null) : IAttackTargetModifier
        {
            public string Name => "TwoHits";
            public int GetComplexity(PowerContext powerContext) => 1;
            public PowerCost GetCost(AttackContext attackContext) =>
                new PowerCost(Multiplier: 2, SingleTargetMultiplier: 1) 
                + (EffectModifier?.GetCost(SameAsOtherTarget.FindContextAt(attackContext)) ?? PowerCost.Empty); // Because both attacks can hit the same target, SingleTargetMultiplier needs to be 1

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
                if (EffectModifier == null)
                    return null;
                var effectContext = SameAsOtherTarget.FindContextAt(attackContext);
                var origMutator = EffectModifier.GetTargetInfoMutator(effectContext);
                if (origMutator == null)
                    return null;

                return new TargetInfoMutator(100, (targetInfo) =>
                {
                    var tempTarget = origMutator.Apply(targetInfo);
                    return targetInfo with
                    {
                        AdditionalSentences = targetInfo.AdditionalSentences.Add($"If both of your attacks hit the same target, the target is also {OxfordComma(tempTarget.Parts.ToArray())}".FinishSentence())
                    };
                });
            }

            public IEnumerable<IAttackTargetModifier> GetUpgrades(UpgradeStage stage, AttackContext attackContext)
            {
                var effectContext = SameAsOtherTarget.FindContextAt(attackContext);
                if (EffectModifier == null)
                {
                    return from formula in ModifierDefinitions.effectModifiers
                           from mod in formula.GetBaseModifiers(stage, effectContext)
                           where !attackContext.Modifiers.Any(m => m.Name == mod.Name)
                           select this with { EffectModifier = mod };
                }

                return from upgrade in EffectModifier.GetUpgrades(stage, effectContext)
                       select this with { EffectModifier = upgrade };
            }

            public IEnumerable<Lens<IModifier, IModifier>> GetNestedModifiers()
            {
                if (EffectModifier == null)
                    yield break;
                yield return Lens<IModifier>.To<IModifier>(
                    mod => mod is TwoHitsModifier twoHits ? twoHits.EffectModifier! : throw new InvalidOperationException(),
                    (mod, newMod) => mod is TwoHitsModifier twoHits && newMod is IEffectModifier effectModifier ? twoHits with { EffectModifier = effectModifier } : throw new InvalidOperationException()
                );
            }

            public IAttackTargetModifier Finalize(AttackContext powerContext) => this;

        }

        // Identical attacks against up to 3 targets.
        public record UpToThreeTargetsModifier() : IAttackTargetModifier
        {
            public string Name => "UpToThreeTargets";
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
