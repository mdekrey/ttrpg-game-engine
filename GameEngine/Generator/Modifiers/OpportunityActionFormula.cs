using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using GameEngine.Rules;

namespace GameEngine.Generator.Modifiers
{
    public record OpportunityActionFormula() : PowerModifierFormula(ModifierName)
    {
        public const string ModifierName = "OpportunityAction";

        public override IEnumerable<RandomChances<IPowerModifier>> GetOptions(PowerProfileBuilder power)
        {
            if (this.HasModifier(power) || power.PowerInfo.Usage == PowerFrequency.AtWill) yield break;

            var cost = new PowerCost(PowerGenerator.GetBasePower(power.PowerInfo.Level, power.PowerInfo.Usage) - PowerGenerator.GetBasePower(power.PowerInfo.Level, power.PowerInfo.Usage - 1));

            yield return new(new OpportunityActionModifier(cost));
        }

        public record OpportunityActionModifier(PowerCost Cost) : PowerModifier(ModifierName)
        {
            public override int GetComplexity() => 0;

            public override PowerCost GetCost() => Cost;

            public override IEnumerable<RandomChances<IPowerModifier>> GetUpgrades(PowerHighLevelInfo powerInfo, IEnumerable<IPowerModifier> modifiers) =>
                Enumerable.Empty<RandomChances<IPowerModifier>>();
            public override SerializedEffect Apply(SerializedEffect effect, PowerProfile powerProfile)
            {
                // TODO
                return effect;
            }
        }
    }
}
