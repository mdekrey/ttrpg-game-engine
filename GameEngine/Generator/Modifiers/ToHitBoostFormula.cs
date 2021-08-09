using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using GameEngine.Rules;
using static GameEngine.Generator.ImmutableConstructorExtension;

namespace GameEngine.Generator.Modifiers
{
    public record ToHitBoostFormula(ImmutableList<string> Keywords) : PowerModifierFormula(Keywords, "To-Hit Bonus")
    {
        public override IEnumerable<ApplicablePowerModifierFormula> GetOptions(AttackProfileBuilder attack, PowerHighLevelInfo powerInfo)
        {
            if (HasModifier(attack)) yield break;
            var amounts = new GameDiceExpression[] { 2 }.Concat(powerInfo.ToolProfile.Abilities.Select(a => (GameDiceExpression)a));
            var targets = new[] { "self", "nearest ally", "an ally within 5 squares" };
            foreach (var amount in amounts)
            {
                foreach (var target in targets)
                {
                    yield return new(BuildModifier(new PowerCost(0.5), amount, "next attack", target));
                    yield return new(BuildModifier(new PowerCost(0.5), amount, "the target", target));
                }
            }

            PowerModifier BuildModifier(PowerCost powerCost, GameDiceExpression amount, string condition, string target) =>
                new (Name, powerCost, Build(
                    ("Amount", amount.ToString()),
                    ("Condition", condition),
                    ("Target", target)
                ));
        }

        public override SerializedEffect Apply(SerializedEffect effect, PowerProfile powerProfile, AttackProfile attackProfile, PowerModifier modifier)
        {
            // TODO
            return effect;
        }
    }
}
