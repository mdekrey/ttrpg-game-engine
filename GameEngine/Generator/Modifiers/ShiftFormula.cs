using System.Collections.Generic;
using System.Collections.Immutable;
using GameEngine.Rules;
using static GameEngine.Generator.ImmutableConstructorExtension;

namespace GameEngine.Generator.Modifiers
{
    public record ShiftFormula(ImmutableList<string> Keywords) : PowerModifierFormula(Keywords, "Shift")
    {
        public ShiftFormula(params string[] keywords) : this(keywords.ToImmutableList()) { }

        public override IEnumerable<ApplicablePowerModifierFormula> GetOptions(AttackProfileBuilder attack, PowerHighLevelInfo powerInfo)
        {
            if (HasModifier(attack)) yield break;
            // TODO - allow sliding allies, fixed numbers, etc.
            yield return new(new PowerCost(0.5), BuildModifier((GameDiceExpression)powerInfo.ToolProfile.Abilities[0]));

            PowerModifier BuildModifier(GameDiceExpression amount) =>
                new PowerModifier(Name, Build(
                    ("Amount", amount.ToString())
                ));
        }

        public override SerializedEffect Apply(SerializedEffect effect, PowerProfile powerProfile, AttackProfile attackProfile, PowerModifier modifier)
        {
            return effect with { Slide = new SerializedSlide(Amount: modifier.Options["Amount"]) };
        }
    }

}
