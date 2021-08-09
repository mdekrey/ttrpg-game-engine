using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using GameEngine.Rules;
using static GameEngine.Generator.ImmutableConstructorExtension;

namespace GameEngine.Generator.Modifiers
{
    public record ToHitBoostFormula(ImmutableList<string> Keywords) : PowerModifierFormula(Keywords, ModifierName)
    {
        public const string ModifierName = "To-Hit Bonus";
        public override IEnumerable<RandomChances<PowerModifier>> GetOptions(AttackProfileBuilder attack, PowerHighLevelInfo powerInfo)
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

            ToHitBoost BuildModifier(PowerCost powerCost, GameDiceExpression amount, string condition, string target) =>
                new(powerCost, amount, condition, target);
        }

        public record ToHitBoost(PowerCost Cost, GameDiceExpression Amount, string Condition, string Target) : PowerModifier(ModifierName)
        {
            public override int GetComplexity() => 1;

            public override PowerCost GetCost() => Cost;

            public override SerializedEffect Apply(SerializedEffect effect, PowerProfile powerProfile, AttackProfile attackProfile)
            {
                // TODO
                return effect;
            }
        }
    }
}
