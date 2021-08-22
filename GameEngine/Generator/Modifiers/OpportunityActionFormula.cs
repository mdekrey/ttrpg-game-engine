using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using GameEngine.Rules;

namespace GameEngine.Generator.Modifiers
{
    public record OpportunityActionFormula() : PowerModifierFormula(ModifierName)
    {
        public const string ModifierName = "OpportunityAction";

        public override bool IsValid(PowerProfileBuilder builder) => builder.PowerInfo.Usage != PowerFrequency.AtWill;
        public override IPowerModifier GetBaseModifier(PowerProfileBuilder power)
        {
            return new MaybeOpportunityActionModifier();
        }

        public record MaybeOpportunityActionModifier() : PowerModifier(ModifierName)
        {
            public override int GetComplexity() => 0;

            public override PowerCost GetCost(PowerProfileBuilder builder) => PowerCost.Empty;

            public override IEnumerable<IPowerModifier> GetUpgrades(PowerProfileBuilder power, UpgradeStage stage) =>
                new[] { new OpportunityActionModifier() };
            public override SerializedEffect Apply(SerializedEffect effect, PowerProfile powerProfile) => effect;
        }

        public record OpportunityActionModifier() : PowerModifier(ModifierName)
        {
            public override int GetComplexity() => 0;

            public override PowerCost GetCost(PowerProfileBuilder builder) => new PowerCost(PowerGenerator.GetBasePower(builder.PowerInfo.Level, builder.PowerInfo.Usage) - PowerGenerator.GetBasePower(builder.PowerInfo.Level, builder.PowerInfo.Usage - 1));

            public override IEnumerable<IPowerModifier> GetUpgrades(PowerProfileBuilder power, UpgradeStage stage) =>
                Enumerable.Empty<IPowerModifier>();
            public override SerializedEffect Apply(SerializedEffect effect, PowerProfile powerProfile)
            {
                // TODO - apply effect
                return effect;
            }
        }
    }
}
