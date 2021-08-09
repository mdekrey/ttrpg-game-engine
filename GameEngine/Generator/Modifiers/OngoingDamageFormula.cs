using System.Collections.Generic;
using System.Collections.Immutable;
using GameEngine.Rules;
using static GameEngine.Generator.ImmutableConstructorExtension;

namespace GameEngine.Generator.Modifiers
{
    public record OngoingDamageFormula(ImmutableList<string> Keywords) : PowerModifierFormula(Keywords, ModifierName)
    {
        public const string ModifierName = "Ongoing Damage";
        public override IEnumerable<RandomChances<PowerModifier>> GetOptions(AttackProfileBuilder attack, PowerHighLevelInfo powerInfo)
        {
            if (HasModifier(attack)) yield break;

            var amounts = new[] { 5, 10 };
            foreach (var amount in amounts)
            {
                yield return new(BuildModifier(amount, new PowerCost(amount / 2.5)));
            }

            OngoingDamage BuildModifier(GameDiceExpression amount, PowerCost Cost) =>
                new (Cost, amount);
        }

        public record OngoingDamage(PowerCost Cost, GameDiceExpression Amount) : PowerModifier(ModifierName)
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
