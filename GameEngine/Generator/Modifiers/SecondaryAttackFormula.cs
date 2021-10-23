using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using GameEngine.Rules;
using static GameEngine.Generator.ImmutableConstructorExtension;
using static GameEngine.Generator.PowerBuildingExtensions;
using static GameEngine.Generator.ProseHelpers;

namespace GameEngine.Generator.Modifiers
{
    public record MultiattackFormula() : PowerModifierFormula(ModifierName)
    {
        public const string ModifierName = "Multiattack";

        public override bool IsValid(PowerProfileBuilder builder) => true;
        public override IEnumerable<IPowerModifier> GetBaseModifiers(UpgradeStage stage, PowerProfileBuilder power) =>
            new ShouldMultiattackModifier().GetUpgrades(stage, power);

        public record ShouldMultiattackModifier() : PowerModifier(ModifierName)
        {
            public override int GetComplexity(PowerHighLevelInfo powerInfo) => 1;
            public override PowerCost GetCost(PowerProfileBuilder builder) => PowerCost.Empty;
            public override bool IsPlaceholder() => true;
            public override bool MustUpgrade() => true;
            public override IEnumerable<IPowerModifier> GetUpgrades(UpgradeStage stage, PowerProfileBuilder power)
            {
                if (stage != UpgradeStage.AttackSetup)
                    yield break;
                if (power.Attacks.Count > 1)
                    yield break;

                yield return new DelegateModifier(new TwoHitsModifier());
                yield return new DelegateModifier(new UpToThreeTargetsModifier());

                foreach (var count in new[] { 2, 3 })
                {
                    yield return new SplitAttackModifier(count, false);
                    yield return new SplitAttackModifier(count, true);
                }
            }
        
            public override PowerTextMutator? GetTextMutator(PowerProfile power) => throw new NotSupportedException("Should be upgraded or removed before this point");
        }

        public record DelegateModifier(IAttackModifier AttackModifier) : PowerModifier(ModifierName)
        {
            public override int GetComplexity(PowerHighLevelInfo powerInfo) => AttackModifier.GetComplexity(powerInfo);
            public override PowerCost GetCost(PowerProfileBuilder power) => AttackModifier.GetCost(power.Attacks[0]);

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
                            Modifiers = attack.Modifiers.Add(AttackModifier),
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

            public override PowerCost GetCost(AttackProfileBuilder builder) => PowerCost.Empty;
            public override bool IsPlaceholder() => false;
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

            public override PowerCost GetCost(AttackProfileBuilder builder) =>
                new PowerCost(Multiplier: 1 / FollowupAttackPower, SingleTargetMultiplier: 1 / FollowupAttackPower);
            public override bool IsPlaceholder() => false;
            public override IEnumerable<IAttackModifier> GetUpgrades(UpgradeStage stage, AttackProfileBuilder attack, PowerProfileBuilder power) =>
                Enumerable.Empty<IAttackModifier>();
            public override AttackInfoMutator? GetAttackInfoMutator(PowerProfile power) => null;
        }

        // Two Identical attacks
        public record TwoHitsModifier() : AttackModifier(ModifierName)
        {
            // TODO - modifiers if both hit
            public override int GetComplexity(PowerHighLevelInfo powerInfo) => 1;
            public const string ModifierName = "TwoHits";
            public override PowerCost GetCost(AttackProfileBuilder builder) =>
                new PowerCost(Multiplier: 2, SingleTargetMultiplier: 1); // Because both attacks can hit the same target, SingleTargetMultiplier needs to be 1
            public override bool IsPlaceholder() => false;
            public override IEnumerable<IAttackModifier> GetUpgrades(UpgradeStage stage, AttackProfileBuilder attack, PowerProfileBuilder power) =>
                Enumerable.Empty<IAttackModifier>();
            public override double ApplyEffectiveWeaponDice(double weaponDice) => weaponDice * 2;
            public override AttackInfoMutator? GetAttackInfoMutator(PowerProfile power) =>
                new(0, (attack, index) => attack with
                {
                    TargetType = AttackType.Target.OneOrTwoCreatures,
                    AttackNotes = ", two attacks"
                });
        }

        // Identical attacks against up to 3 targets.
        public record UpToThreeTargetsModifier() : AttackModifier(ModifierName)
        {
            public override int GetComplexity(PowerHighLevelInfo powerInfo) => 0;
            public const string ModifierName = "UpToThreeTargets";
            public override PowerCost GetCost(AttackProfileBuilder builder) => new PowerCost(1.5);
            public override bool IsPlaceholder() => false;
            public override IEnumerable<IAttackModifier> GetUpgrades(UpgradeStage stage, AttackProfileBuilder attack, PowerProfileBuilder power) =>
                Enumerable.Empty<IAttackModifier>();
            public override AttackInfoMutator? GetAttackInfoMutator(PowerProfile power) =>
                new(0, (attack, index) => attack with
                {
                    TargetType = AttackType.Target.OneTwoOrThreeCreatures,
                    AttackNotes = ", one attack per target"
                });
        }
    }
}
