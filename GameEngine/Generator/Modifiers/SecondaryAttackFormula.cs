using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
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

        public IEnumerable<IPowerModifier> GetBaseModifiers(UpgradeStage stage, PowerProfileBuilder power) =>
            new ShouldMultiattackModifier().GetUpgrades(stage, power);

        public record ShouldMultiattackModifier() : PowerModifier(ModifierName)
        {
            public override int GetComplexity(PowerHighLevelInfo powerInfo) => 1;
            public override PowerCost GetCost(PowerProfileBuilder builder) => PowerCost.Empty;
            public override bool IsPlaceholder() => true;
            public override bool MustUpgrade() => true;
            public override bool CanUseRemainingPower() => true;

            public override IEnumerable<IPowerModifier> GetUpgrades(UpgradeStage stage, PowerProfileBuilder power)
            {
                if (stage != UpgradeStage.Standard)
                    yield break;
                if (power.Attacks.Count > 1)
                    yield break;

                yield return new TargetDelegateModifier(new TwoHitsModifier());
                yield return new TargetDelegateModifier(new UpToThreeTargetsModifier());

                foreach (var count in new[] { 2, 3 })
                {
                    yield return new SplitAttackModifier(count, false);
                    yield return new SplitAttackModifier(count, true);
                }
            }
        
            public override PowerTextMutator? GetTextMutator(PowerProfile power) => throw new NotSupportedException("Should be upgraded or removed before this point");
        }

        public record TargetDelegateModifier(ITargetModifier TargetModifier) : PowerModifier(ModifierName)
        {
            public override int GetComplexity(PowerHighLevelInfo powerInfo) => TargetModifier.GetComplexity(powerInfo);
            public override PowerCost GetCost(PowerProfileBuilder power) => TargetModifier.GetCost(power.Attacks[0].TargetEffects[0], power);

            public override IEnumerable<IPowerModifier> GetUpgrades(UpgradeStage stage, PowerProfileBuilder attack) =>
                Enumerable.Empty<IPowerModifier>();

            public override IEnumerable<PowerProfileBuilder> TrySimplifySelf(PowerProfileBuilder power)
            {
                var attack = power.Attacks.Single();
                yield return power with
                {
                    Attacks = new[] {
                        attack with
                        {
                            TargetEffects = attack.TargetEffects.SetItem(0, attack.TargetEffects[0] with { Target = TargetModifier }),
                        }
                    }.ToImmutableList(),
                    Modifiers = power.Modifiers.Remove(this).Add(new MultiattackAppliedModifier()),
                };
            }
            public override PowerTextMutator? GetTextMutator(PowerProfile power) => throw new NotSupportedException("Should be upgraded or removed before this point");
        }

        public record SplitAttackModifier(int AmountsCount, bool RequiresPreviousHit) : PowerModifier(ModifierName)
        {
            public override int GetComplexity(PowerHighLevelInfo powerInfo) => RequiresPreviousHit ? 1 : 2;

            public override PowerCost GetCost(PowerProfileBuilder power) =>
                PowerCost.Empty;
            public override bool IsPlaceholder() => true;

            public override IEnumerable<IPowerModifier> GetUpgrades(UpgradeStage stage, PowerProfileBuilder attack) =>
                Enumerable.Empty<IPowerModifier>();

            public override IEnumerable<PowerProfileBuilder> TrySimplifySelf(PowerProfileBuilder power)
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
                    Modifiers = power.Modifiers.Remove(this).Add(new MultiattackAppliedModifier()),
                };
            }
            public override PowerTextMutator? GetTextMutator(PowerProfile power) => throw new NotSupportedException("Should be upgraded or removed before this point");
        }

        public record MultiattackAppliedModifier() : PowerModifier(ModifierName)
        {
            public override int GetComplexity(PowerHighLevelInfo powerInfo) => 0;
            public override PowerCost GetCost(PowerProfileBuilder builder) => PowerCost.Empty;
            public override bool IsPlaceholder() => true;
            public override IEnumerable<IPowerModifier> GetUpgrades(UpgradeStage stage, PowerProfileBuilder power) => Enumerable.Empty<IPowerModifier>();
            public override PowerTextMutator? GetTextMutator(PowerProfile power) => throw new NotSupportedException("Should be upgraded or removed before this point");
        }

        public record MustHitForNextAttackModifier() : AttackModifier(MustHitForNextAttackModifier.ModifierName)
        {
            public override int GetComplexity(PowerHighLevelInfo powerInfo) => 0;

            public const string ModifierName = "RequiredHitForNextAttack";

            public override PowerCost GetCost(AttackProfileBuilder builder, PowerProfileBuilder power) => PowerCost.Empty;
            public override bool IsPlaceholder() => false;
            public override bool CanUseRemainingPower() => true;
            public override IEnumerable<IAttackModifier> GetUpgrades(UpgradeStage stage, AttackProfileBuilder attack, PowerProfileBuilder power) =>
                Enumerable.Empty<IAttackModifier>();

            public override AttackInfoMutator? GetAttackInfoMutator(PowerProfile power) =>
                new(int.MaxValue, (attack, index) => attack with
                {
                    HitSentences = attack.HitSentences.Add($"Make a {Ordinal(index + 1)} attack."),
                });
        }

        public record SecondaryAttackModifier() : AttackModifier(SecondaryAttackModifier.ModifierName)
        {
            public const double FollowupAttackPower = 1.5;

            public override int GetComplexity(PowerHighLevelInfo powerInfo) => 0;

            public const string ModifierName = "RequiresPreviousHit";

            public override PowerCost GetCost(AttackProfileBuilder builder, PowerProfileBuilder power) =>
                new PowerCost(Multiplier: 1 / FollowupAttackPower, SingleTargetMultiplier: 1 / FollowupAttackPower);
            public override bool IsPlaceholder() => false;
            public override bool CanUseRemainingPower() => true;
            public override IEnumerable<IAttackModifier> GetUpgrades(UpgradeStage stage, AttackProfileBuilder attack, PowerProfileBuilder power) =>
                Enumerable.Empty<IAttackModifier>();
            public override AttackInfoMutator? GetAttackInfoMutator(PowerProfile power) => null;
        }

        // Two Identical attacks
        public record TwoHitsModifier(IEffectModifier? EffectModifier = null) : ITargetModifier
        {
            public string Name => "TwoHits";
            public int GetComplexity(PowerHighLevelInfo powerInfo) => 1;
            public PowerCost GetCost(TargetEffect builder, PowerProfileBuilder context) =>
                new PowerCost(Multiplier: 2, SingleTargetMultiplier: 1) + (EffectModifier?.GetCost(builder, context) ?? PowerCost.Empty); // Because both attacks can hit the same target, SingleTargetMultiplier needs to be 1

            public AttackType GetAttackType(PowerProfile power, int? attackIndex) =>
                (power.Tool, power.ToolRange) switch
                {
                    (ToolType.Weapon, ToolRange.Melee) => new MeleeWeaponAttackType(),
                    (ToolType.Implement, ToolRange.Melee) => new MeleeTouchAttackType(),
                    (ToolType.Weapon, ToolRange.Range) => new RangedWeaponAttackType(),
                    (ToolType.Implement, ToolRange.Range) => new RangedAttackType(10),
                    _ => throw new NotSupportedException(),
                };

            public Target GetTarget() => Target.Ally | Target.Enemy;

            public string GetTargetText(PowerProfile power, int? attackIndex) => "One or two creatures";
            public string? GetAttackNotes(PowerProfile power, int? attackIndex) => "two attacks";
            public bool CanUseRemainingPower() => true;
            public TargetInfoMutator? GetTargetInfoMutator(TargetEffect targetEffect, PowerProfile power)
            {
                if (EffectModifier == null)
                    return null;
                var origMutator = EffectModifier.GetTargetInfoMutator(targetEffect, power);
                if (origMutator == null)
                    return null;

                return new TargetInfoMutator(100, (targetInfo) =>
                {
                    var tempTarget = origMutator.Apply(new TargetInfo(
                        Target: targetInfo.Target,
                        AttackType: targetInfo.AttackType,
                        AttackNotes: targetInfo.AttackNotes,
                        DamageExpression: targetInfo.DamageExpression,
                        Parts: targetInfo.Parts,
                        AdditionalSentences: targetInfo.AdditionalSentences
                    ));
                    return targetInfo with
                    {
                        AdditionalSentences = targetInfo.AdditionalSentences.Add($"If both of your attacks hit the same target, the target is also {OxfordComma(tempTarget.Parts.ToArray())}".FinishSentence())
                    };
                });
            }

            public IEnumerable<ITargetModifier> GetUpgrades(UpgradeStage stage, TargetEffect target, PowerProfileBuilder power, int? attackIndex)
            {
                if (EffectModifier == null)
                {
                    return from formula in ModifierDefinitions.effectModifiers
                           from mod in formula.GetBaseModifiers(stage, target, null, power)
                           where !target.Modifiers.Any(m => m.Name == mod.Name)
                           select this with { EffectModifier = mod };
                }

                return from upgrade in EffectModifier.GetUpgrades(stage, target, null, power)
                       select this with { EffectModifier = upgrade };
            }

            public IEnumerable<IModifier> GetNestedModifiers()
            {
                if (EffectModifier == null)
                    yield break;
                yield return EffectModifier;
            }

        }

        // Identical attacks against up to 3 targets.
        public record UpToThreeTargetsModifier() : ITargetModifier
        {
            public string Name => "UpToThreeTargets";
            public int GetComplexity(PowerHighLevelInfo powerInfo) => 0;
            public PowerCost GetCost(TargetEffect builder, PowerProfileBuilder context) => new PowerCost(1.5);

            public AttackType GetAttackType(PowerProfile power, int? attackIndex) =>
                (power.Tool, power.ToolRange) switch
                {
                    (ToolType.Weapon, ToolRange.Melee) => new MeleeWeaponAttackType(),
                    (ToolType.Implement, ToolRange.Melee) => new MeleeTouchAttackType(),
                    (ToolType.Weapon, ToolRange.Range) => new RangedWeaponAttackType(),
                    (ToolType.Implement, ToolRange.Range) => new RangedAttackType(10),
                    _ => throw new NotSupportedException(),
                };

            public Target GetTarget() => Target.Ally | Target.Enemy;

            public string GetTargetText(PowerProfile power, int? attackIndex) => "One, two, or three creatures";
            public string? GetAttackNotes(PowerProfile power, int? attackIndex) => "one attack per target";
            public bool CanUseRemainingPower() => true;
            public TargetInfoMutator? GetTargetInfoMutator(TargetEffect targetEffect, PowerProfile power) => null;

            public IEnumerable<ITargetModifier> GetUpgrades(UpgradeStage stage, TargetEffect target, PowerProfileBuilder power, int? attackIndex) =>
                Enumerable.Empty<ITargetModifier>();
        }
    }
}
