using System.Collections.Generic;
using System.Collections.Immutable;
using GameEngine.Rules;

namespace GameEngine.Generator.Modifiers
{
    public record OpportunityActionFormula(ImmutableList<string> Keywords) : PowerModifierFormula(Keywords, ModifierName)
    {
        public const string ModifierName = "OpportunityAction";

        public override IEnumerable<RandomChances<PowerModifier>> GetOptions(AttackProfileBuilder attack, PowerHighLevelInfo powerInfo)
        {
            if (HasModifier(attack)) yield break;

            var cost = new PowerCost(PowerGenerator.GetBasePower(powerInfo.Level, powerInfo.Usage) - PowerGenerator.GetBasePower(powerInfo.Level, powerInfo.Usage - 1));

            yield return new(new OpportunityActionModifier(cost));
        }

        public record OpportunityActionModifier(PowerCost Cost) : PowerModifier(ModifierName)
        {
            public override PowerCost GetCost() => Cost;

            public override SerializedEffect Apply(SerializedEffect effect, PowerProfile powerProfile, AttackProfile attackProfile)
            {
                // TODO
                return effect;
            }
        }
    }
}
