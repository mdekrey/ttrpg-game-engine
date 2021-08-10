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
            foreach (var entry in attack.PowerInfo.ToolProfile.Abilities.Where(a => a != attack.Ability))
                yield return new(BuildModifier(new PowerCost(0.5), (GameDiceExpression)entry), Chances: 1);
            yield return new(BuildModifier(new PowerCost(0.5), 2), Chances: 5);

            ToHitBonus BuildModifier(PowerCost powerCost, GameDiceExpression dice) =>
                new (powerCost, dice);
        }

        public record ToHitBonus(PowerCost Cost, GameDiceExpression Amount) : PowerModifier(ModifierName)
        {
            public override int GetComplexity() => 1;

            public override PowerCost GetCost() => Cost;

            public override IEnumerable<RandomChances<PowerModifier>> GetUpgrades(AttackProfileBuilder attack) =>
                // TODO
                Enumerable.Empty<RandomChances<PowerModifier>>();
            public override SerializedEffect Apply(SerializedEffect effect, PowerProfile powerProfile, AttackProfile attackProfile)
            {
                return Pipe(
                    (AttackRollOptions attack) => attack with
                    {
                        Bonus = (GameDiceExpression.Parse(attack.Bonus) + Amount).ToString()
                    },
                    ModifyAttack,
                    ModifyTarget
                )(effect);
            }
        }
    }

}
