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

        public override IEnumerable<ApplicablePowerModifierFormula> GetOptions(AttackProfileBuilder attack, PowerHighLevelInfo powerInfo)
        {
            if (HasModifier(attack) || HasModifier(attack, BurstFormula.ModifierName)) yield break;

            var available = attack.WeaponDice;

            for (var (current, counter) = (attack.Cost.Minimum, 1); current <= available / 2; (current, counter) = (current + 0.5, counter * 2))
            {
                var (a, b) = (current, available - current);
                var costA = new PowerCost(Fixed: a);
                var costB = new PowerCost(Fixed: b);
                if (a * 2 < b)
                    yield return new(BuildModifier(costA, isFollowUp: true), Chances: counter * (a == b ? 2 : 1));
                yield return new(BuildModifier(costA), Chances: counter * (a == b ? 2 : 1));
                if (a != b)
                    yield return new(BuildModifier(costB), Chances: counter);
            }

            PowerModifier BuildModifier(PowerCost powerCost, bool isFollowUp = false) =>
                new PowerModifier(Name, powerCost, Build(
                    ("IsFollowUp", isFollowUp.ToString())
                ));
        }

        public override SerializedEffect Apply(SerializedEffect effect, PowerProfile powerProfile, AttackProfile attackProfile, PowerModifier modifier)
        {
            // This modifier is a special case and should be removed to create an extra attack
            throw new System.NotSupportedException();
        }

        internal static PowerModifier? NeedToSplit(AttackProfileBuilder attack)
        {
            return attack.Modifiers.FirstOrDefault(m => m.Modifier == ModifierName);
        }

        internal static (AttackProfileBuilder original, AttackProfileBuilder secondary) Unapply(AttackProfileBuilder attack, PowerModifier secondaryAttackModifier)
        {
            var cost = secondaryAttackModifier.Cost.Fixed;
            var isFollowUp = bool.Parse(secondaryAttackModifier.Options["IsFollowUp"]);
            return (
                attack with 
                { 
                    Modifiers = attack.Modifiers.Where(m => m.Modifier != ModifierName).ToImmutableList(), 
                    Cost = attack.Cost with 
                    { 
                        Initial = attack.Cost.Initial - cost,
                    }
                },
                attack with
                {
                    Modifiers = isFollowUp
                        ? Build(new PowerModifier(SecondaryAttackFormula.ModifierName, PowerCost.Empty))
                        : ImmutableList<PowerModifier>.Empty,
                    Cost = attack.Cost with
                    {
                        Initial = isFollowUp ? cost * 2 : cost,
                    },
                }
            );
        }
    }

    public record SecondaryAttackFormula(ImmutableList<string> Keywords) : PowerModifierFormula(Keywords, ModifierName)
    {
        public const string ModifierName = "SecondaryAttack";
        // Only applies if previous attack hits
        public override IEnumerable<ApplicablePowerModifierFormula> GetOptions(AttackProfileBuilder attack, PowerHighLevelInfo powerInfo)
        {
            yield break;
        }

        public override SerializedEffect Apply(SerializedEffect effect, PowerProfile powerProfile, AttackProfile attackProfile, PowerModifier modifier)
        {
            // TODO
            return effect;
        }
    }

}
