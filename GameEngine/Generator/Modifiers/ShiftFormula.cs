using System.Collections.Generic;
using System.Collections.Immutable;
using GameEngine.Rules;
using static GameEngine.Generator.ImmutableConstructorExtension;

namespace GameEngine.Generator.Modifiers
{
    public record ShiftFormula() : PowerModifierFormula(ModifierName)
    {
        public const string ModifierName = "Shift";

        public override IEnumerable<RandomChances<PowerModifier>> GetOptions(AttackProfileBuilder attack, PowerHighLevelInfo powerInfo)
        {
            if (HasModifier(attack)) yield break;
            // TODO - allow sliding allies, fixed numbers, etc.
            yield return new(BuildModifier((GameDiceExpression)powerInfo.ToolProfile.Abilities[0], new PowerCost(0.5)));

            ShiftModifier BuildModifier(GameDiceExpression amount, PowerCost cost) =>
                new (cost, amount);
        }

        public record ShiftModifier(PowerCost Cost, GameDiceExpression Amount) : PowerModifier(ModifierName)
        {
            public override int GetComplexity() => 1;

            public override PowerCost GetCost() => Cost;

            public override SerializedEffect Apply(SerializedEffect effect, PowerProfile powerProfile, AttackProfile attackProfile)
            {
                return effect with { Slide = new SerializedSlide(Amount: Amount.ToString()) };
            }
        }
    }

}
