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
            public override PowerCost GetCost() => PowerCost.Empty;
            public override SerializedEffect Apply(SerializedEffect effect, PowerProfile powerProfile) => effect;
            public override IEnumerable<RandomChances<IPowerModifier>> GetUpgrades(PowerProfileBuilder power)
            {
                var available = power.Attacks.Select(a => a.WeaponDice).DefaultIfEmpty(power.Limits.Initial).Sum();

                // TODO - two hits
                // TODO - two targets
                // TODO - three targets
                // TODO - secondary attack
                // TODO - secondary and tertiary attack
                //yield return new(new DelegateModifier(new UpToThreeTargetsModifier()));
                for (var (current, counter) = (power.Limits.Minimum, 1); current <= available / 2; (current, counter) = (current + 0.5, counter * 2))
                {
                    var (a, b) = (current, available - current);
                    if (a * 2 < b)
                    {
                        yield return new(BuildModifier(a, isFollowUp: true), Chances: counter * (a == b ? 2 : 1));
                    }
                    yield return new(BuildModifier(a), Chances: counter * (a == b ? 2 : 1));
                    if (a != b)
                        yield return new(BuildModifier(b), Chances: counter);
                }

                SplitAttackModifier BuildModifier(double cost, bool isFollowUp = false) =>
                    new(cost, isFollowUp);
            }
        }

        public record DelegateModifier(IAttackModifier AttackModifier) : PowerModifier(ModifierName)
        {
            public override int GetComplexity() => 0;
            public override PowerCost GetCost() => PowerCost.Empty;

            public override IEnumerable<RandomChances<IPowerModifier>> GetUpgrades(PowerProfileBuilder attack) =>
                Enumerable.Empty<RandomChances<IPowerModifier>>();
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

        public record SplitAttackModifier(double Cost, bool IsFollowUp) : PowerModifier(ModifierName)
        {
            public override int GetComplexity() => IsFollowUp ? 2 : 1;

            public override PowerCost GetCost() => new (Fixed: Cost);

            public override IEnumerable<RandomChances<IPowerModifier>> GetUpgrades(PowerProfileBuilder attack) =>
                Enumerable.Empty<RandomChances<IPowerModifier>>();
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
                    Attacks = new[] {
                        attack with
                        {
                            Modifiers = attack.Modifiers.ToImmutableList(),
                            Limits = attack.Limits with
                            {
                                Initial = attack.Limits.Initial - Cost,
                                MaxComplexity = attack.Limits.MaxComplexity - GetComplexity(),
                            }
                        },
                        attack with
                        {
                            Modifiers = IsFollowUp
                                ? Build<IAttackModifier>(new SecondaryAttackModifier())
                                : ImmutableList<IAttackModifier>.Empty,
                            Limits = attack.Limits with
                            {
                                Initial = IsFollowUp ? Cost * 2 : Cost,
                                MaxComplexity = attack.Limits.MaxComplexity - (2 - GetComplexity()),
                            },
                        },
                    }.ToImmutableList(),
                    Modifiers = power.Modifiers.Remove(this).Add(new MultiattackAppliedModifier()),
                };
            }
        }

        public record MultiattackAppliedModifier() : PowerModifier(ModifierName)
        {
            public override SerializedEffect Apply(SerializedEffect effect, PowerProfile powerProfile) => effect;
            public override int GetComplexity() => 0;
            public override PowerCost GetCost() => PowerCost.Empty;
            public override IEnumerable<RandomChances<IPowerModifier>> GetUpgrades(PowerProfileBuilder power) => Enumerable.Empty<RandomChances<IPowerModifier>>();
        }

        public record SecondaryAttackModifier() : AttackModifier(SecondaryAttackModifier.ModifierName)
        {
            public override int GetComplexity() => 0;

            public const string ModifierName = "SecondaryAttack";

            public override PowerCost GetCost() => PowerCost.Empty;
            public override bool IsMetaModifier() => true;
            public override IEnumerable<RandomChances<IAttackModifier>> GetUpgrades(AttackProfileBuilder attack) =>
                Enumerable.Empty<RandomChances<IAttackModifier>>();
            public override SerializedEffect Apply(SerializedEffect effect, PowerProfile powerProfile, AttackProfile attackProfile)
            {
                // TODO
                return effect;
            }
        }

        public record UpToThreeTargetsModifier() : AttackModifier(UpToThreeTargetsModifier.ModifierName)
        {
            public override int GetComplexity() => 0;
            public const string ModifierName = "UpToThreeTargets";
            public override PowerCost GetCost() => new PowerCost(1);
            public override bool IsMetaModifier() => true;
            public override IEnumerable<RandomChances<IAttackModifier>> GetUpgrades(AttackProfileBuilder attack) =>
                Enumerable.Empty<RandomChances<IAttackModifier>>();
            public override SerializedEffect Apply(SerializedEffect effect, PowerProfile powerProfile, AttackProfile attackProfile)
            {
                // TODO
                return effect;
            }
        }
    }
}
