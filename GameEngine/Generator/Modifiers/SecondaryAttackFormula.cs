using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using GameEngine.Rules;
using static GameEngine.Generator.ImmutableConstructorExtension;

namespace GameEngine.Generator.Modifiers
{
    public record MultiattackFormula(ImmutableList<string> Keywords) : PowerModifierFormula(Keywords, ModifierName)
    {
        public const string ModifierName = "Multiattack";

        public override IEnumerable<RandomChances<PowerModifier>> GetOptions(AttackProfileBuilder attack, PowerHighLevelInfo powerInfo)
        {
            if (HasModifier(attack) || HasModifier(attack, BurstFormula.ModifierName)) yield break;

            var available = attack.WeaponDice;

            for (var (current, counter) = (attack.Cost.Minimum, 1); current <= available / 2; (current, counter) = (current + 0.5, counter * 2))
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

        public record MultiattackModifier(double Cost, bool IsFollowUp) : PowerModifier(ModifierName)
        {
            public override int GetComplexity() => 0;

            public override PowerCost GetCost() => new (Fixed: Cost);

            public override SerializedEffect Apply(SerializedEffect effect, PowerProfile powerProfile, AttackProfile attackProfile)
            {
                // This modifier is a special case and should be removed to create an extra attack
                throw new System.NotSupportedException();
            }


            public (AttackProfileBuilder original, AttackProfileBuilder secondary) Unapply(AttackProfileBuilder attack)
            {
                return (
                    attack with
                    {
                        Modifiers = attack.Modifiers.Where(m => m.Name != ModifierName).ToImmutableList(),
                        Cost = attack.Cost with
                        {
                            Initial = attack.Cost.Initial - Cost,
                        }
                    },
                    attack with
                    {
                        Modifiers = IsFollowUp
                            ? Build<PowerModifier>(new SecondaryAttackModifier())
                            : ImmutableList<PowerModifier>.Empty,
                        Cost = attack.Cost with
                        {
                            Initial = IsFollowUp ? Cost * 2 : Cost,
                        },
                    }
                );
            }
        }

        internal static MultiattackModifier? NeedToSplit(AttackProfileBuilder attack)
        {
            return attack.Modifiers.OfType<MultiattackModifier>().FirstOrDefault();
        }

        public record SecondaryAttackModifier() : PowerModifier(SecondaryAttackModifier.ModifierName)
        {
            public override int GetComplexity() => 0;

            public const string ModifierName = "SecondaryAttack";

            public override PowerCost GetCost() => PowerCost.Empty;
            public override SerializedEffect Apply(SerializedEffect effect, PowerProfile powerProfile, AttackProfile attackProfile)
            {
                // TODO
                return effect;
            }
        }
    }
}
