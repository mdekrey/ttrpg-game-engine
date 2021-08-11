using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using GameEngine.Rules;

namespace GameEngine.Generator.Modifiers
{
    public record OpportunityActionFormula() : PowerModifierFormula(ModifierName)
    {
        public const string ModifierName = "OpportunityAction";

        public override IEnumerable<RandomChances<PowerModifier>> GetOptions(AttackProfileBuilder attack)
        {
            if (HasModifier(attack) || attack.PowerInfo.Usage == PowerFrequency.AtWill) yield break;

            var cost = new PowerCost(PowerGenerator.GetBasePower(attack.PowerInfo.Level, attack.PowerInfo.Usage) - PowerGenerator.GetBasePower(attack.PowerInfo.Level, attack.PowerInfo.Usage - 1));

            yield return new(new OpportunityActionModifier(cost));
        }

        public record OpportunityActionModifier(PowerCost Cost) : PowerModifier(ModifierName)
        {
            public override int GetComplexity() => 0;

            public override PowerCost GetCost() => Cost;

            public override IEnumerable<RandomChances<PowerModifier>> GetUpgrades(AttackProfileBuilder attack) =>
                Enumerable.Empty<RandomChances<PowerModifier>>();
            public override SerializedEffect Apply(SerializedEffect effect, PowerProfile powerProfile, AttackProfile attackProfile)
            {
                // TODO
                return effect;
            }
        }
    }
}
