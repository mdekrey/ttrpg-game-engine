﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using GameEngine.Rules;
using static GameEngine.Generator.ImmutableConstructorExtension;
using static GameEngine.Generator.PowerBuildingExtensions;

namespace GameEngine.Generator.Modifiers
{
    public record MultiattackFormula() : PowerModifierFormula(ModifierName)
    {
        public const string ModifierName = "Multiattack";

        public override bool IsValid(PowerProfileBuilder builder) => true;
        public override IPowerModifier GetBaseModifier(PowerProfileBuilder attack) =>
            new ShouldMultiattackModifier();

        public record ShouldMultiattackModifier() : PowerModifier(ModifierName)
        {
            public override int GetComplexity() => 1;
            public override PowerCost GetCost(PowerProfileBuilder builder) => PowerCost.Empty;
            public override SerializedEffect Apply(SerializedEffect effect, PowerProfile powerProfile) => effect;
            public override IEnumerable<IPowerModifier> GetUpgrades(PowerProfileBuilder power, UpgradeStage stage)
            {
                if (power.Attacks.Count > 1)
                    yield break;
                var allocated = power.Attacks.Select(a => a.TotalCost.Fixed).Sum();
                var available = power.Attacks.Select(a => a.WeaponDice).DefaultIfEmpty(power.Limits.Initial).Sum();

                yield return new DelegateModifier(new TwoHitsModifier(), limit => limit with { Minimum = Math.Max(1, limit.Minimum / 2), Initial = limit.Initial / 2, });
                yield return new DelegateModifier(new UpToThreeTargetsModifier());

                var options = (from set in new[]
                              {
                                  new[] { available / 2, available / 2 },
                                  new[] { available / 2 - 1, available / 2 + 1 },
                                  new[] { available / 3, available / 3, available / 3 },
                                  new[] { available / 3 - 1, available / 3, available / 3 + 1 },
                              }
                              where !set.Any(v => v < 1)
                              from reversed in new[] { set, set.Reverse().ToArray() }
                              let withAllocated = reversed.Take(reversed.Length - 1).Concat(new[] { reversed.Last() + allocated })
                              select new EquatableImmutableList<double>(withAllocated)).Distinct();
                foreach (var option in options)
                {
                    if (option.Skip(1).Any(o => o != option[0]))
                        yield return new SplitAttackModifier(option.ToImmutableList(), false);
                    yield return new SplitAttackModifier(option.ToImmutableList(), true);
                }
            }
        }

        public record DelegateModifier(IAttackModifier AttackModifier, Transform<AttackLimits>? limitTransform = null) : PowerModifier(ModifierName)
        {
            public override int GetComplexity() => AttackModifier.GetComplexity();
            public override PowerCost GetCost(PowerProfileBuilder builder) => AttackModifier.GetCost(builder.Attacks[0]);

            public override IEnumerable<IPowerModifier> GetUpgrades(PowerProfileBuilder attack, UpgradeStage stage) =>
                Enumerable.Empty<IPowerModifier>();
            public override SerializedEffect Apply(SerializedEffect effect, PowerProfile powerProfile)
            {
                // This modifier is a special case and should be removed to create an extra attack
                throw new System.NotSupportedException();
            }

            public override PowerProfileBuilder TryApplyToProfileAndRemove(PowerProfileBuilder power)
            {
                var attack = power.Attacks.Single();
                return power with
                {
                    Attacks = new[] {
                        attack with
                        {
                            Limits = limitTransform?.Invoke(attack.Limits) ?? attack.Limits,
                            Modifiers = attack.Modifiers.Add(AttackModifier),
                        }
                    }.ToImmutableList(),
                    Modifiers = power.Modifiers.Remove(this).Add(new MultiattackAppliedModifier()),
                };
            }
        }

        public record SplitAttackModifier(ImmutableList<double> Amounts, bool RequiresPreviousHit) : PowerModifier(ModifierName)
        {
            public override int GetComplexity() => RequiresPreviousHit ? 1 : 2;

            public override PowerCost GetCost(PowerProfileBuilder builder) =>
                new (Fixed: Amounts.Take(Amounts.Count - 1).Select((v, i) => v * (i == 0 || !RequiresPreviousHit ? 1 : 0.5)).Sum() - builder.Attacks.Select(a => a.TotalCost.Fixed).Sum());

            public override IEnumerable<IPowerModifier> GetUpgrades(PowerProfileBuilder attack, UpgradeStage stage) =>
                Enumerable.Empty<IPowerModifier>();
            public override SerializedEffect Apply(SerializedEffect effect, PowerProfile powerProfile)
            {
                // This modifier is a special case and should be removed to create an extra attack
                throw new System.NotSupportedException();
            }

            public override PowerProfileBuilder TryApplyToProfileAndRemove(PowerProfileBuilder power)
            {
                var attack = power.Attacks.Single();
                return power with
                {
                    Attacks = Amounts.Select((cost, index) =>
                        attack with
                        {
                            Modifiers = index switch
                            {
                                0 => ImmutableList<IAttackModifier>.Empty,
                                _ when index < Amounts.Count - 1 && RequiresPreviousHit => Build<IAttackModifier>(new SecondaryAttackModifier()),
                                _ when RequiresPreviousHit => attack.Modifiers.ToImmutableList().Add(new SecondaryAttackModifier()),
                                _ => attack.Modifiers.ToImmutableList()
                            },
                            Limits = attack.Limits with
                            {
                                Minimum = index switch
                                {
                                    > 0 when RequiresPreviousHit => Math.Max(1, Math.Ceiling(attack.Limits.Minimum / Amounts.Count)) * 2,
                                    _ => Math.Max(1, Math.Ceiling(attack.Limits.Minimum / Amounts.Count)),
                                },
                                Initial = index switch
                                {
                                    > 0 when RequiresPreviousHit => cost * 2,
                                    _ => cost,
                                },
                                MaxComplexity = index switch { 0 => 0, _ => attack.Limits.MaxComplexity - GetComplexity() },
                            }
                        }
                    ).ToImmutableList(),
                    Modifiers = power.Modifiers.Remove(this).Add(new MultiattackAppliedModifier()),
                };
            }
        }

        public record MultiattackAppliedModifier() : PowerModifier(ModifierName)
        {
            public override SerializedEffect Apply(SerializedEffect effect, PowerProfile powerProfile) => effect;
            public override int GetComplexity() => 0;
            public override PowerCost GetCost(PowerProfileBuilder builder) => PowerCost.Empty;
            public override IEnumerable<IPowerModifier> GetUpgrades(PowerProfileBuilder power, UpgradeStage stage) => Enumerable.Empty<IPowerModifier>();
        }

        public record SecondaryAttackModifier() : AttackModifier(SecondaryAttackModifier.ModifierName)
        {
            public override int GetComplexity() => 0;

            public const string ModifierName = "RequiresPreviousHit";

            public override PowerCost GetCost(AttackProfileBuilder builder) => new PowerCost(Multiplier: 0.5);
            public override bool IsMetaModifier() => true;
            public override IEnumerable<IAttackModifier> GetUpgrades(AttackProfileBuilder attack, UpgradeStage stage) =>
                Enumerable.Empty<IAttackModifier>();
            public override SerializedEffect Apply(SerializedEffect effect, PowerProfile powerProfile, AttackProfile attackProfile)
            {
                // TODO - apply effect
                return effect;
            }
        }

        // Two Identical attacks
        public record TwoHitsModifier() : AttackModifier(ModifierName)
        {
            // TODO - modifiers if both hit
            public override int GetComplexity() => 0;
            public const string ModifierName = "TwoHits";
            public override PowerCost GetCost(AttackProfileBuilder builder) => PowerCost.Empty;
            public override bool IsMetaModifier() => true;
            public override IEnumerable<IAttackModifier> GetUpgrades(AttackProfileBuilder attack, UpgradeStage stage) =>
                Enumerable.Empty<IAttackModifier>();
            public override SerializedEffect Apply(SerializedEffect effect, PowerProfile powerProfile, AttackProfile attackProfile)
            {
                // TODO - apply effect
                return effect;
            }
            public override double ApplyEffectiveWeaponDice(double weaponDice) => weaponDice * 2;
        }

        // Identical attacks against up to 3 targets.
        public record UpToThreeTargetsModifier() : AttackModifier(ModifierName)
        {
            public override int GetComplexity() => 0;
            public const string ModifierName = "UpToThreeTargets";
            public override PowerCost GetCost(AttackProfileBuilder builder) => new PowerCost(1.5);
            public override bool IsMetaModifier() => true;
            public override IEnumerable<IAttackModifier> GetUpgrades(AttackProfileBuilder attack, UpgradeStage stage) =>
                Enumerable.Empty<IAttackModifier>();
            public override SerializedEffect Apply(SerializedEffect effect, PowerProfile powerProfile, AttackProfile attackProfile)
            {
                // TODO - apply effect
                return effect;
            }
        }
    }
}
