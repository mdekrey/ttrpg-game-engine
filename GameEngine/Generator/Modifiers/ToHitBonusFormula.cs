using System.Collections.Generic;
using System.Text;
using System.Collections.Immutable;
using GameEngine.Rules;
using System.Linq;
using static GameEngine.Generator.ImmutableConstructorExtension;
using static GameEngine.Generator.PowerBuildingExtensions;

namespace GameEngine.Generator.Modifiers
{

    public record ToHitBonusFormula(ImmutableList<string> Keywords) : PowerModifierFormula(Keywords, ModifierName)
    {
        public const string ModifierName = "To-Hit Bonus to Current Attack";
        public ToHitBonusFormula(params string[] keywords) : this(keywords.ToImmutableList()) { }

        public override IEnumerable<RandomChances<PowerModifier>> GetOptions(AttackProfileBuilder attack, PowerHighLevelInfo powerInfo)
        {
            if (HasModifier(attack)) yield break;
            foreach (var entry in powerInfo.ToolProfile.Abilities.Where(a => a != attack.Ability))
                yield return new(BuildModifier(new PowerCost(0.5), (GameDiceExpression)entry), Chances: 1);
            yield return new(BuildModifier(new PowerCost(0.5), 2), Chances: 5);

            ToHitBonus BuildModifier(PowerCost powerCost, GameDiceExpression dice) =>
                new (powerCost, dice);
        }

        public record ToHitBonus(PowerCost Cost, GameDiceExpression Amount) : PowerModifier(ModifierName)
        {
            public override PowerCost GetCost() => Cost;

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
