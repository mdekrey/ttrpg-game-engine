using System.Collections.Generic;
using System.Text;
using System.Collections.Immutable;
using GameEngine.Rules;
using System.Linq;
using static GameEngine.Generator.ImmutableConstructorExtension;
using static GameEngine.Generator.PowerBuildingExtensions;

namespace GameEngine.Generator.Modifiers
{

    public record ToHitBonusFormula(ImmutableList<string> Keywords) : PowerModifierFormula(Keywords, "To-Hit Bonus to Current Attack")
    {
        public ToHitBonusFormula(params string[] keywords) : this(keywords.ToImmutableList()) { }

        public override IEnumerable<ApplicablePowerModifierFormula> GetOptions(AttackProfileBuilder attack, PowerHighLevelInfo powerInfo)
        {
            if (HasModifier(attack)) yield break;
            foreach (var entry in powerInfo.ToolProfile.Abilities.Where(a => a != attack.Ability))
                yield return new(BuildModifier(new PowerCost(0.5), (GameDiceExpression)entry), Chances: 1);
            yield return new(BuildModifier(new PowerCost(0.5), 2), Chances: 5);

            PowerModifier BuildModifier(PowerCost powerCost, GameDiceExpression dice) =>
                new (Name, powerCost, Build(("Amount", dice.ToString())));
        }

        public override SerializedEffect Apply(SerializedEffect effect, PowerProfile powerProfile, AttackProfile attackProfile, PowerModifier modifier)
        {
            return Pipe(
                (AttackRollOptions attack) => attack with
                {
                    Bonus = (GameDiceExpression.Parse(attack.Bonus) + GameDiceExpression.Parse(modifier.Options["Amount"])).ToString()
                },
                ModifyAttack,
                ModifyTarget
            )(effect);
        }
    }

}
