using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using GameEngine.Rules;
using static GameEngine.Generator.ImmutableConstructorExtension;

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

                // TODO - secondary attack
                // TODO - secondary and tertiary attack
                yield return new DelegateModifier(new TwoHitsModifier());
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
                    yield return new SplitAttackModifier(option.ToImmutableList(), false);
                    yield return new SplitAttackModifier(option.ToImmutableList(), true);
                }
            }
        }

        public record DelegateModifier(IAttackModifier AttackModifier) : PowerModifier(ModifierName)
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
                            Modifiers = attack.Modifiers.Add(AttackModifier),
                        }
                    }.ToImmutableList(),
                    Modifiers = power.Modifiers.Remove(this).Add(new MultiattackAppliedModifier()),
                };
            }
        }

        public record SplitAttackModifier(ImmutableList<double> Amounts, bool RequiresPreviousHit) : PowerModifier(ModifierName)
        {
            public override int GetComplexity() => RequiresPreviousHit ? 2 : 1;

            public override PowerCost GetCost(PowerProfileBuilder builder) => new (Fixed: Amounts.Select((v, i) => v * (i == 0 || !RequiresPreviousHit ? 1 : 0.5)).Sum() - builder.Attacks.Select(a => a.TotalCost.Fixed).Sum());

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
                // TODO - better complexity
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
                                Initial = cost,
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
            public override PowerCost GetCost(AttackProfileBuilder builder) => new PowerCost(Multiplier: 2);
            public override bool IsMetaModifier() => true;
            public override IEnumerable<IAttackModifier> GetUpgrades(AttackProfileBuilder attack, UpgradeStage stage) =>
                Enumerable.Empty<IAttackModifier>();
            public override SerializedEffect Apply(SerializedEffect effect, PowerProfile powerProfile, AttackProfile attackProfile)
            {
                // TODO - apply effect
                return effect;
            }
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
