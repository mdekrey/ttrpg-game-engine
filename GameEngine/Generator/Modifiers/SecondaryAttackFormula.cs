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

            var available = attack.Cost.Result;

            for (var (current, counter) = (attack.Cost.Minimum, 1); current <= available / 2; (current, counter) = (current + 0.5, counter * 2))
            {
                var (a, b) = (current, available - current);
                var costA = new PowerCost(Fixed: b);
                var costB = new PowerCost(Fixed: a);
                if (a * 2 < b)
                    yield return new(costB, BuildModifier(nextAttack: a * 2, original: b, cost: costB.Fixed, isFollowUp: true), Chances: counter * (a == b ? 2 : 1));
                yield return new(costB, BuildModifier(nextAttack: a, original: b, cost: costB.Fixed), Chances: counter * (a == b ? 2 : 1));
                if (a != b)
                    yield return new(costA, BuildModifier(nextAttack: b, original: a, cost: costA.Fixed), Chances: counter);
            }

            PowerModifier BuildModifier(double nextAttack, double original, double cost, bool isFollowUp = false) =>
                new PowerModifier(Name, Build(
                    ("NextAttack", nextAttack.ToString("0.0")),
                    ("Remaining", original.ToString("0.0")),
                    ("Cost", cost.ToString("0.0")),
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

        internal static (AttackProfileBuilder original, AttackProfileBuilder secondary) Unapply(AttackProfileBuilder attack, ImmutableDictionary<string, string> secondaryAttackOptions)
        {
            var nextAttack = double.Parse(secondaryAttackOptions["NextAttack"]);
            var remaining = double.Parse(secondaryAttackOptions["Remaining"]);
            var cost = double.Parse(secondaryAttackOptions["Cost"]);
            var isFollowUp = bool.Parse(secondaryAttackOptions["IsFollowUp"]);
            return (
                attack with 
                { 
                    Modifiers = attack.Modifiers.Where(m => m.Modifier != ModifierName).ToImmutableList(), 
                    Cost = attack.Cost with 
                    { 
                        CurrentCost = attack.Cost.CurrentCost with { Fixed = attack.Cost.CurrentCost.Fixed - cost },
                        Initial = attack.Cost.Initial - cost,
                    }
                },
                attack with
                {
                    Modifiers = isFollowUp
                        ? Build(new PowerModifier(SecondaryAttackFormula.ModifierName))
                        : ImmutableList<PowerModifier>.Empty,
                    Cost = attack.Cost with
                    {
                        CurrentCost = PowerCost.Empty,
                        Initial = nextAttack,
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
