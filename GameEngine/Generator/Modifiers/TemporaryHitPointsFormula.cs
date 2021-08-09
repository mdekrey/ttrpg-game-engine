using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using GameEngine.Rules;
using static GameEngine.Generator.ImmutableConstructorExtension;

namespace GameEngine.Generator.Modifiers
{
    public record TemporaryHitPointsFormula(ImmutableList<string> Keywords) : PowerModifierFormula(Keywords, "+Temporary Hit Points")
    {
        public override IEnumerable<RandomChances<PowerModifier>> GetOptions(AttackProfileBuilder attack, PowerHighLevelInfo powerInfo)
        {
            if (HasModifier(attack)) yield break;
            var amounts = powerInfo.ToolProfile.Abilities.Select(a => (GameDiceExpression)a);
            var targets = new[] { "self", "nearest ally", "an ally within 5 squares" };
            foreach (var amount in amounts)
            {
                foreach (var target in targets)
                {
                    yield return new(BuildModifier(new PowerCost(1), amount, target));
                }
            }

            PowerModifier BuildModifier(PowerCost powerCost, GameDiceExpression amount, string target) =>
                new (Name, powerCost, Build(
                    ("Amount", amount.ToString()),
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
