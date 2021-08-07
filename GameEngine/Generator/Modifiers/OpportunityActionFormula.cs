using System.Collections.Generic;
using System.Collections.Immutable;
using GameEngine.Rules;

namespace GameEngine.Generator.Modifiers
{
    public record OpportunityActionFormula(ImmutableList<string> Keywords) : PowerModifierFormula(Keywords, ModifierName)
    {
        public const string ModifierName = "OpportunityAction";

        public override IEnumerable<ApplicablePowerModifierFormula> GetOptions(AttackProfileBuilder attack, PowerHighLevelInfo powerInfo)
        {
            if (HasModifier(attack)) yield break;

            var cost = new PowerCost(PowerGenerator.GetBasePower(powerInfo.Level, powerInfo.Usage) - PowerGenerator.GetBasePower(powerInfo.Level, powerInfo.Usage - 1));

            yield return new(cost, new PowerModifier(Name));
        }

        public override SerializedEffect Apply(SerializedEffect effect, PowerProfile powerProfile, AttackProfile attackProfile, PowerModifier modifier)
        {
            // TODO
            return effect;
        }
    }
}
