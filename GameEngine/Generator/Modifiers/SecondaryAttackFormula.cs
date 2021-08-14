﻿using System;
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
                for (var (current, counter) = (power.Limits.Minimum, 1); current <= available / 2; (current, counter) = (current + 0.5, counter * 2))
                {
                    var (a, b) = (current, available - current);
                    if (a * 2 < b)
                        yield return new(BuildModifier(a, isFollowUp: true), Chances: counter * (a == b ? 2 : 1));
                    yield return new(BuildModifier(a), Chances: counter * (a == b ? 2 : 1));
                    if (a != b)
                        yield return new(BuildModifier(b), Chances: counter);
                }

                MultiattackModifier BuildModifier(double cost, bool isFollowUp = false) =>
                    new(cost, isFollowUp);
            }
        }

        public record MultiattackModifier(double Cost, bool IsFollowUp) : PowerModifier(ModifierName)
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


            public IEnumerable<AttackProfileBuilder> Split(AttackProfileBuilder attack)
            {
                // TODO - better complexity
                return new[] {
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
                };
            }
        }

        internal static MultiattackModifier? NeedToSplit(PowerProfileBuilder power)
        {
            return power.Modifiers.OfType<MultiattackModifier>().FirstOrDefault();
        }

        public record SecondaryAttackModifier() : AttackModifier(SecondaryAttackModifier.ModifierName)
        {
            public override int GetComplexity() => 0;

            public const string ModifierName = "SecondaryAttack";

            public override PowerCost GetCost() => PowerCost.Empty;
            public override bool IsMetaModifier() => true;
            public override IEnumerable<RandomChances<IAttackModifier>> GetUpgrades(AttackProfileBuilder attack) =>
                // TODO
                Enumerable.Empty<RandomChances<IAttackModifier>>();
            public override SerializedEffect Apply(SerializedEffect effect, PowerProfile powerProfile, AttackProfile attackProfile)
            {
                // TODO
                return effect;
            }
        }
    }
}
