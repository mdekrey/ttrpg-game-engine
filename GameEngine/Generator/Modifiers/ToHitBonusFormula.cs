using System.Collections.Generic;
using System.Text;
using System.Collections.Immutable;
using GameEngine.Rules;
using System.Linq;
using static GameEngine.Generator.ImmutableConstructorExtension;
using static GameEngine.Generator.PowerBuildingExtensions;

namespace GameEngine.Generator.Modifiers
{

    public record ToHitBonusFormula() : PowerModifierFormula(ModifierName)
    {
        public const string ModifierName = "To-Hit Bonus to Current Attack";

        public override IEnumerable<RandomChances<PowerModifier>> GetOptions(AttackProfileBuilder attack)
        {
            if (HasModifier(attack)) yield break;
            yield return new(new ToHitBonus(2), Chances: 5);
            foreach (var entry in attack.PowerInfo.ToolProfile.Abilities.Where(a => a != attack.Ability))
                yield return new(new ToHitBonus((GameDiceExpression)entry), Chances: 1);
        }

        public record ToHitBonus(GameDiceExpression Amount) : PowerModifier(ModifierName)
        {
            public override int GetComplexity() => 1;

            public override PowerCost GetCost() => new PowerCost(Amount.ToWeaponDice());

            public override IEnumerable<RandomChances<PowerModifier>> GetUpgrades(AttackProfileBuilder attack)
            {
                if (Amount.Abilities == CharacterAbilities.Empty)
                {
                    if (Amount.DieCodes.Modifier < 8) // actually 10
                        yield return new(this with { Amount = Amount.StepUpModifier() });
                    if (Amount.DieCodes.Modifier == 2)
                    {
                        foreach (var ability in attack.PowerInfo.ToolProfile.Abilities.Where(a => a != attack.Ability))
                            yield return new(this with { Amount = Amount + ability });
                    }
                }
            }
            public override SerializedEffect Apply(SerializedEffect effect, PowerProfile powerProfile, AttackProfile attackProfile)
            {
                return Pipe(
                    (AttackRollOptions attack) => attack with
                    {
                        Bonus = (GameDiceExpression.Parse(attack.Bonus) + Amount.RoundModifier()).ToString()
                    },
                    ModifyAttack,
                    ModifyTarget
                )(effect);
            }
        }
    }

}
