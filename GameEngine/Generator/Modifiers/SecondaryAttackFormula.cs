using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using GameEngine.Rules;
using static GameEngine.Generator.ImmutableConstructorExtension;

namespace GameEngine.Generator.Modifiers
{
    public record SecondaryAttackFormula(ImmutableList<string> Keywords) : PowerModifierFormula(Keywords, ModifierName)
    {
        public const string ModifierName = "Multiattack";

        public override IEnumerable<ApplicablePowerModifierFormula> GetApplicable(AttackProfileBuilder attack, PowerHighLevelInfo powerInfo)
        {
            if (HasModifier(attack) || HasModifier(attack, BurstFormula.ModifierName)) yield break;


            for (var (current, counter) = (attack.Cost.Minimum, 1); current <= attack.Cost.Initial / 2; (current, counter) = (current + 0.5, counter * 2))
            {
                var cost = new PowerCost(Multiplier: 1 - (current / attack.Cost.Initial));
                var (a, b) = (current, attack.Cost.Initial - current);
                yield return new(cost, BuildModifier(nextAttack: a, original: b), Chances: counter * (a == b ? 2 : 1));
                if (a != b)
                    yield return new(cost with { Multiplier = 1 - cost.Multiplier }, BuildModifier(nextAttack: b, original: a), Chances: counter);
            }

            PowerModifier BuildModifier(double nextAttack, double original) =>
                new PowerModifier(Name, Build(
                    ("NextAttack", nextAttack.ToString("0.0")),
                    ("Remaining", original.ToString("0.0"))
                ));
        }

        public override SerializedEffect Apply(SerializedEffect effect, PowerProfile powerProfile, AttackProfile attackProfile, PowerModifier modifier)
        {
            // This modifier is a special case and should be removed to create an extra attack
            throw new System.NotSupportedException();
        }

        internal static (AttackProfileBuilder original, AttackProfileBuilder secondary) Unapply(AttackProfileBuilder attack, ImmutableDictionary<string, string> secondaryAttackOptions)
        {
            var nextAttack = double.Parse(secondaryAttackOptions["NextAttack"]);
            var remaining = double.Parse(secondaryAttackOptions["Remaining"]);
            return (
                attack with 
                { 
                    Modifiers = attack.Modifiers.Where(m => m.Modifier != SecondaryAttackFormula.ModifierName).ToImmutableList(), 
                    Cost = attack.Cost with 
                    { 
                        CurrentCost = attack.Cost.CurrentCost with { Multiplier = attack.Cost.CurrentCost.Multiplier * (nextAttack + remaining) / remaining },
                        Initial = remaining,
                    }
                },
                attack with
                {
                    Modifiers = ImmutableList<PowerModifier>.Empty,
                    Cost = attack.Cost with
                    {
                        CurrentCost = PowerCost.Empty,
                        Initial = nextAttack,
                    },
                }
            );
        }
    }
}
